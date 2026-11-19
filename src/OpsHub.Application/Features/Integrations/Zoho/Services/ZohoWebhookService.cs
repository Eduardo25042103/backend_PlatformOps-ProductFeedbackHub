using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpsHub.Application.Common.Exceptions;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Features.Integrations.Zoho.DTOs;
using OpsHub.Application.Features.ProductFeedback.Services;
using OpsHub.Domain.Entities.Commercial;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Application.Features.Integrations.Zoho.Services;

public class ZohoWebhookService : IZohoWebhookService
{
    private readonly IApplicationDbContext _context;
    private readonly IZohoSignatureValidator _signatureValidator;
    private readonly ILogger<ZohoWebhookService> _logger;

    public ZohoWebhookService(
        IApplicationDbContext context,
        IZohoSignatureValidator signatureValidator,
        ILogger<ZohoWebhookService> logger)
    {
        _context = context;
        _signatureValidator = signatureValidator;
        _logger = logger;
    }

    public async Task<ZohoWebhookResult> ProcessWebhookAsync(
        ZohoWebhookPayload payload,
        string rawBody,
        string? signatureHeader,
        CancellationToken ct = default)
    {
        // 1. Validar firma criptográfica HMAC-SHA256
        if (!_signatureValidator.ValidateSignature(rawBody, signatureHeader))
        {
            _logger.LogWarning("Intento de webhook de Zoho con firma inválida o ausente.");
            throw new UnauthorizedAccessException("Firma criptográfica de Zoho CRM inválida en la cabecera 'X-Zoho-Signature'.");
        }

        var ruc = payload.Ruc?.Trim();
        if (string.IsNullOrWhiteSpace(ruc))
            throw new Common.Exceptions.ValidationException("Ruc", "El RUC del cliente es obligatorio en el payload de Zoho.");

        // 2. Buscar o crear el Cliente SaaS
        var cliente = await _context.ClientesSaas.FirstOrDefaultAsync(c => c.Ruc == ruc, ct);
        if (cliente == null)
        {
            var defaultPlan = await _context.PlanesSuscripcion.OrderBy(p => p.Id).FirstAsync(ct);
            cliente = new ClienteSaas
            {
                Ruc = ruc,
                RazonSocial = string.IsNullOrWhiteSpace(payload.RazonSocial) ? $"Cliente RUC {ruc}" : payload.RazonSocial.Trim(),
                PlanId = defaultPlan.Id,
                MontoAnualContratado = payload.MontoAnual,
                EstadoCuenta = "ACTIVO",
                CreatedAt = DateTime.UtcNow
            };
            _context.ClientesSaas.Add(cliente);
            await _context.SaveChangesAsync(ct);
        }
        else if (payload.MontoAnual > 0)
        {
            cliente.MontoAnualContratado = payload.MontoAnual;
            cliente.UpdatedAt = DateTime.UtcNow;
        }

        // 3. Buscar o registrar la Sugerencia de Producto
        var tituloRequerimiento = payload.RequerimientoTitulo.Trim();
        var sugerencia = await _context.SugerenciasProducto
            .Include(s => s.Votos)
            .Include(s => s.ClientesSolicitantes)
            .Include(s => s.Priorizacion)
            .FirstOrDefaultAsync(s => s.Titulo.ToLower() == tituloRequerimiento.ToLower(), ct);

        if (sugerencia == null)
        {
            var estadoEvaluacion = await _context.EstadosSugerencia.OrderBy(e => e.Orden).FirstAsync(ct);
            var usuarioAdmin = await _context.UsuariosInternos.OrderBy(u => u.Id).FirstAsync(ct);

            var totalYear = await _context.SugerenciasProducto.CountAsync(ct);
            var correlativo = $"FEAT-{DateTime.UtcNow.Year}-{(totalYear + 1).ToString("D3")}";

            sugerencia = new SugerenciaProducto
            {
                Codigo = correlativo,
                Titulo = tituloRequerimiento,
                Descripcion = string.IsNullOrWhiteSpace(payload.RequerimientoDescripcion) ? tituloRequerimiento : payload.RequerimientoDescripcion.Trim(),
                ModuloAfectado = string.IsNullOrWhiteSpace(payload.ModuloAfectado) ? "GENERAL" : payload.ModuloAfectado.Trim().ToUpperInvariant(),
                EstadoId = estadoEvaluacion.Id,
                CreadoPorId = usuarioAdmin.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.SugerenciasProducto.Add(sugerencia);
            await _context.SaveChangesAsync(ct);
        }

        // 4. Vincular el cliente solicitante con el peso económico (ARR)
        var vinculacion = await _context.ClientesSolicitantes
            .FirstOrDefaultAsync(cs => cs.SugerenciaId == sugerencia.Id && cs.ClienteId == cliente.Id, ct);

        if (vinculacion != null)
        {
            vinculacion.ValorAnualRepresentado = payload.MontoAnual;
            vinculacion.OrigenWebhook = "ZOHO_CRM";
        }
        else
        {
            _context.ClientesSolicitantes.Add(new ClienteSolicitante
            {
                SugerenciaId = sugerencia.Id,
                ClienteId = cliente.Id,
                ValorAnualRepresentado = payload.MontoAnual,
                OrigenWebhook = "ZOHO_CRM",
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync(ct);

        // 5. Recalcular Priorización
        var allSolicitantes = await _context.ClientesSolicitantes
            .Where(cs => cs.SugerenciaId == sugerencia.Id)
            .ToListAsync(ct);
        var totalArr = allSolicitantes.Sum(cs => cs.ValorAnualRepresentado);
        var totalVotos = await _context.VotosSugerencia.CountAsync(v => v.SugerenciaId == sugerencia.Id, ct);

        var priorizacion = await _context.PriorizacionesSugerencia
            .FirstOrDefaultAsync(p => p.SugerenciaId == sugerencia.Id, ct);

        var score = SugerenciaService.CalcularPuntuacion(
            priorizacion?.ImpactoComercialScore ?? 5,
            totalArr,
            totalVotos,
            priorizacion?.EsfuerzoTiDias ?? 1);

        if (priorizacion == null)
        {
            var usuarioAdmin = await _context.UsuariosInternos.OrderBy(u => u.Id).FirstAsync(ct);
            priorizacion = new PriorizacionSugerencia
            {
                SugerenciaId = sugerencia.Id,
                EsfuerzoTiDias = 1,
                ImpactoComercialScore = 5,
                PuntuacionFinal = score,
                EvaluadoPorId = usuarioAdmin.Id,
                UpdatedAt = DateTime.UtcNow
            };
            _context.PriorizacionesSugerencia.Add(priorizacion);
        }
        else
        {
            priorizacion.PuntuacionFinal = score;
            priorizacion.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        return new ZohoWebhookResult
        {
            Processed = true,
            Message = "Requerimiento comercial de Zoho CRM procesado y vinculado con éxito.",
            ClienteId = cliente.Id,
            SugerenciaId = sugerencia.Id,
            SugerenciaCodigo = sugerencia.Codigo,
            PuntuacionActualizada = score
        };
    }
}
