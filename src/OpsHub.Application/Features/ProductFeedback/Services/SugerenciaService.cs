using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Exceptions;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Features.Incidents.DTOs;
using OpsHub.Application.Features.ProductFeedback.DTOs;
using OpsHub.Domain.Entities.ProductFeedback;
using ValidationException = OpsHub.Application.Common.Exceptions.ValidationException;

namespace OpsHub.Application.Features.ProductFeedback.Services;

public class SugerenciaService : ISugerenciaService
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<CreateSugerenciaRequest> _createValidator;
    private readonly IValidator<PriorizarSugerenciaRequest> _priorizarValidator;
    private readonly IValidator<AddClienteSolicitanteRequest> _clienteValidator;

    public SugerenciaService(
        IApplicationDbContext context,
        IValidator<CreateSugerenciaRequest> createValidator,
        IValidator<PriorizarSugerenciaRequest> priorizarValidator,
        IValidator<AddClienteSolicitanteRequest> clienteValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _priorizarValidator = priorizarValidator;
        _clienteValidator = clienteValidator;
    }

    public async Task<List<SugerenciaSummaryDto>> GetSugerenciasAsync(SugerenciaFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.SugerenciasProducto
            .AsNoTracking()
            .Include(s => s.Estado)
            .Include(s => s.Votos)
            .Include(s => s.ClientesSolicitantes)
            .Include(s => s.Priorizacion)
            .AsQueryable();

        if (filter.EstadoId.HasValue)
            query = query.Where(s => s.EstadoId == filter.EstadoId.Value);

        if (!string.IsNullOrWhiteSpace(filter.ModuloAfectado))
            query = query.Where(s => s.ModuloAfectado.ToLower() == filter.ModuloAfectado.Trim().ToLower());

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.Codigo.ToLower().Contains(term) ||
                s.Titulo.ToLower().Contains(term) ||
                s.Descripcion.ToLower().Contains(term));
        }

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = await query
            .Select(s => new SugerenciaSummaryDto
            {
                Id = s.Id,
                Codigo = s.Codigo,
                Titulo = s.Titulo,
                ModuloAfectado = s.ModuloAfectado,
                EstadoCodigo = s.Estado.Codigo,
                EstadoNombre = s.Estado.Nombre,
                TotalVotos = s.Votos.Count,
                TotalClientes = s.ClientesSolicitantes.Count,
                TotalArr = s.ClientesSolicitantes.Sum(cs => cs.ValorAnualRepresentado),
                PuntuacionPriorizacion = s.Priorizacion != null ? s.Priorizacion.PuntuacionFinal : 0.00m,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        // Ordenamiento
        items = filter.OrderBy?.ToLower() switch
        {
            "prioridad" => items.OrderByDescending(x => x.PuntuacionPriorizacion).ToList(),
            "votos" => items.OrderByDescending(x => x.TotalVotos).ToList(),
            "arr" => items.OrderByDescending(x => x.TotalArr).ToList(),
            _ => items.OrderByDescending(x => x.CreatedAt).ToList()
        };

        return items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    public async Task<SugerenciaDto> GetSugerenciaByIdAsync(int id, CancellationToken ct = default)
    {
        var sugerencia = await _context.SugerenciasProducto
            .AsNoTracking()
            .Include(s => s.Estado)
            .Include(s => s.CreadoPor)
            .Include(s => s.Votos)
                .ThenInclude(v => v.Usuario)
            .Include(s => s.ClientesSolicitantes)
                .ThenInclude(cs => cs.Cliente)
            .Include(s => s.Priorizacion)
                .ThenInclude(p => p!.EvaluadoPor)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sugerencia == null)
            throw new NotFoundException("SugerenciaProducto", id);

        return MapToDto(sugerencia);
    }

    public async Task<SugerenciaDto> CreateSugerenciaAsync(CreateSugerenciaRequest request, int usuarioId, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var estadoEvaluacion = await _context.EstadosSugerencia
            .FirstOrDefaultAsync(e => e.Codigo == "EVALUACION", ct)
            ?? await _context.EstadosSugerencia.OrderBy(e => e.Orden).FirstAsync(ct);

        var totalYear = await _context.SugerenciasProducto.CountAsync(ct);
        var correlativo = $"FEAT-{DateTime.UtcNow.Year}-{(totalYear + 1).ToString("D3")}";

        var sugerencia = new SugerenciaProducto
        {
            Codigo = correlativo,
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            ModuloAfectado = request.ModuloAfectado.Trim().ToUpperInvariant(),
            EstadoId = estadoEvaluacion.Id,
            CreadoPorId = usuarioId,
            CreatedAt = DateTime.UtcNow
        };

        _context.SugerenciasProducto.Add(sugerencia);
        await _context.SaveChangesAsync(ct);

        // Crear registro inicial de priorización
        var priorizacion = new PriorizacionSugerencia
        {
            SugerenciaId = sugerencia.Id,
            EsfuerzoTiDias = 1,
            ImpactoComercialScore = 1,
            PuntuacionFinal = CalcularPuntuacion(1, 0.00m, 0, 1),
            EvaluadoPorId = usuarioId
        };
        _context.PriorizacionesSugerencia.Add(priorizacion);
        await _context.SaveChangesAsync(ct);

        return await GetSugerenciaByIdAsync(sugerencia.Id, ct);
    }

    public async Task<SugerenciaDto> ToggleVoteAsync(int sugerenciaId, int usuarioId, string? motivoVoto, CancellationToken ct = default)
    {
        var sugerenciaExists = await _context.SugerenciasProducto.AnyAsync(s => s.Id == sugerenciaId, ct);
        if (!sugerenciaExists)
            throw new NotFoundException("SugerenciaProducto", sugerenciaId);

        var votoExistente = await _context.VotosSugerencia
            .FirstOrDefaultAsync(v => v.SugerenciaId == sugerenciaId && v.UsuarioId == usuarioId, ct);

        if (votoExistente != null)
        {
            _context.VotosSugerencia.Remove(votoExistente);
        }
        else
        {
            _context.VotosSugerencia.Add(new VotoSugerencia
            {
                SugerenciaId = sugerenciaId,
                UsuarioId = usuarioId,
                MotivoVoto = motivoVoto?.Trim(),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);
        await RecalcularPriorizacionInternaAsync(sugerenciaId, ct);

        return await GetSugerenciaByIdAsync(sugerenciaId, ct);
    }

    public async Task<SugerenciaDto> AddClienteSolicitanteAsync(int sugerenciaId, AddClienteSolicitanteRequest request, CancellationToken ct = default)
    {
        var validation = await _clienteValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var sugerenciaExists = await _context.SugerenciasProducto.AnyAsync(s => s.Id == sugerenciaId, ct);
        if (!sugerenciaExists)
            throw new NotFoundException("SugerenciaProducto", sugerenciaId);

        var cliente = await _context.ClientesSaas.FirstOrDefaultAsync(c => c.Id == request.ClienteId, ct);
        if (cliente == null)
            throw new NotFoundException("ClienteSaas", request.ClienteId);

        var valorArr = request.ValorAnualRepresentado > 0
            ? request.ValorAnualRepresentado
            : cliente.MontoAnualContratado;

        var vinculacionExistente = await _context.ClientesSolicitantes
            .FirstOrDefaultAsync(cs => cs.SugerenciaId == sugerenciaId && cs.ClienteId == request.ClienteId, ct);

        if (vinculacionExistente != null)
        {
            vinculacionExistente.ValorAnualRepresentado = valorArr;
        }
        else
        {
            _context.ClientesSolicitantes.Add(new ClienteSolicitante
            {
                SugerenciaId = sugerenciaId,
                ClienteId = request.ClienteId,
                ValorAnualRepresentado = valorArr,
                OrigenWebhook = "MANUAL",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);
        await RecalcularPriorizacionInternaAsync(sugerenciaId, ct);

        return await GetSugerenciaByIdAsync(sugerenciaId, ct);
    }

    public async Task<SugerenciaDto> PrioritizeAsync(int sugerenciaId, PriorizarSugerenciaRequest request, int evaluadoPorId, CancellationToken ct = default)
    {
        var validation = await _priorizarValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var sugerencia = await _context.SugerenciasProducto
            .Include(s => s.Votos)
            .Include(s => s.ClientesSolicitantes)
            .FirstOrDefaultAsync(s => s.Id == sugerenciaId, ct);

        if (sugerencia == null)
            throw new NotFoundException("SugerenciaProducto", sugerenciaId);

        var priorizacion = await _context.PriorizacionesSugerencia
            .FirstOrDefaultAsync(p => p.SugerenciaId == sugerenciaId, ct);

        var totalArr = sugerencia.ClientesSolicitantes.Sum(cs => cs.ValorAnualRepresentado);
        var totalVotos = sugerencia.Votos.Count;
        var scoreFinal = CalcularPuntuacion(request.ImpactoComercialScore, totalArr, totalVotos, request.EsfuerzoTiDias);

        if (priorizacion == null)
        {
            priorizacion = new PriorizacionSugerencia
            {
                SugerenciaId = sugerenciaId,
                EsfuerzoTiDias = request.EsfuerzoTiDias,
                ImpactoComercialScore = request.ImpactoComercialScore,
                PuntuacionFinal = scoreFinal,
                EvaluadoPorId = evaluadoPorId,
                UpdatedAt = DateTime.UtcNow
            };
            _context.PriorizacionesSugerencia.Add(priorizacion);
        }
        else
        {
            priorizacion.EsfuerzoTiDias = request.EsfuerzoTiDias;
            priorizacion.ImpactoComercialScore = request.ImpactoComercialScore;
            priorizacion.PuntuacionFinal = scoreFinal;
            priorizacion.EvaluadoPorId = evaluadoPorId;
            priorizacion.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return await GetSugerenciaByIdAsync(sugerenciaId, ct);
    }

    public async Task<List<CatalogItemDto>> GetEstadosSugerenciaAsync(CancellationToken ct = default)
    {
        return await _context.EstadosSugerencia
            .AsNoTracking()
            .OrderBy(e => e.Orden)
            .Select(e => new CatalogItemDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Nombre = e.Nombre
            })
            .ToListAsync(ct);
    }

    private async Task RecalcularPriorizacionInternaAsync(int sugerenciaId, CancellationToken ct)
    {
        var sugerencia = await _context.SugerenciasProducto
            .Include(s => s.Votos)
            .Include(s => s.ClientesSolicitantes)
            .Include(s => s.Priorizacion)
            .FirstOrDefaultAsync(s => s.Id == sugerenciaId, ct);

        if (sugerencia?.Priorizacion != null)
        {
            var totalArr = sugerencia.ClientesSolicitantes.Sum(cs => cs.ValorAnualRepresentado);
            var totalVotos = sugerencia.Votos.Count;
            sugerencia.Priorizacion.PuntuacionFinal = CalcularPuntuacion(
                sugerencia.Priorizacion.ImpactoComercialScore,
                totalArr,
                totalVotos,
                sugerencia.Priorizacion.EsfuerzoTiDias);
            sugerencia.Priorizacion.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
    }

    public static decimal CalcularPuntuacion(int impactoComercialScore, decimal totalArr, int totalVotos, int esfuerzoTiDias)
    {
        var esfuerzo = Math.Max(esfuerzoTiDias, 1);
        var arrBonus = Math.Min((double)totalArr / 1000.0, 50.0);
        var votosBonus = totalVotos * 2.0;
        var baseScore = (impactoComercialScore * 10.0) + arrBonus + votosBonus;
        var finalScore = baseScore / esfuerzo;
        return Math.Round((decimal)finalScore, 2);
    }

    private static SugerenciaDto MapToDto(SugerenciaProducto s)
    {
        return new SugerenciaDto
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Descripcion = s.Descripcion,
            ModuloAfectado = s.ModuloAfectado,
            EstadoId = s.EstadoId,
            EstadoCodigo = s.Estado?.Codigo ?? string.Empty,
            EstadoNombre = s.Estado?.Nombre ?? string.Empty,
            CreadoPorId = s.CreadoPorId,
            CreadoPorNombre = s.CreadoPor != null ? $"{s.CreadoPor.Nombre} {s.CreadoPor.Apellido}" : string.Empty,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,

            TotalVotos = s.Votos.Count,
            TotalClientesSolicitantes = s.ClientesSolicitantes.Count,
            TotalArrRepresentado = s.ClientesSolicitantes.Sum(cs => cs.ValorAnualRepresentado),

            Priorizacion = s.Priorizacion != null ? new PriorizacionDto
            {
                Id = s.Priorizacion.Id,
                EsfuerzoTiDias = s.Priorizacion.EsfuerzoTiDias,
                ImpactoComercialScore = s.Priorizacion.ImpactoComercialScore,
                PuntuacionFinal = s.Priorizacion.PuntuacionFinal,
                EvaluadoPorNombre = s.Priorizacion.EvaluadoPor != null
                    ? $"{s.Priorizacion.EvaluadoPor.Nombre} {s.Priorizacion.EvaluadoPor.Apellido}"
                    : string.Empty,
                UpdatedAt = s.Priorizacion.UpdatedAt
            } : null,

            Votos = s.Votos.Select(v => new VotoDto
            {
                Id = v.Id,
                UsuarioId = v.UsuarioId,
                UsuarioNombre = v.Usuario != null ? $"{v.Usuario.Nombre} {v.Usuario.Apellido}" : string.Empty,
                MotivoVoto = v.MotivoVoto,
                CreatedAt = v.CreatedAt
            }).ToList(),

            ClientesSolicitantes = s.ClientesSolicitantes.Select(cs => new ClienteSolicitanteDto
            {
                Id = cs.Id,
                ClienteId = cs.ClienteId,
                ClienteRazonSocial = cs.Cliente?.RazonSocial ?? string.Empty,
                ClienteRuc = cs.Cliente?.Ruc ?? string.Empty,
                ValorAnualRepresentado = cs.ValorAnualRepresentado,
                OrigenWebhook = cs.OrigenWebhook,
                CreatedAt = cs.CreatedAt
            }).ToList()
        };
    }
}
