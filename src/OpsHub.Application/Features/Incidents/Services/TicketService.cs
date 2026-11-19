using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Exceptions;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Features.Incidents.DTOs;
using OpsHub.Domain.Entities.Incidents;
using ValidationException = OpsHub.Application.Common.Exceptions.ValidationException;

namespace OpsHub.Application.Features.Incidents.Services;

public class TicketService : ITicketService
{
    private readonly IApplicationDbContext _context;
    private readonly IS3StorageService _s3StorageService;
    private readonly IValidator<CreateTicketRequest> _createValidator;
    private readonly IValidator<UpdateTicketStatusRequest> _updateStatusValidator;
    private readonly IValidator<EscalateTicketRequest> _escalateValidator;
    private readonly IValidator<AddCommentRequest> _commentValidator;
    private readonly IValidator<ConfirmAttachmentRequest> _attachmentValidator;

    public TicketService(
        IApplicationDbContext context,
        IS3StorageService s3StorageService,
        IValidator<CreateTicketRequest> createValidator,
        IValidator<UpdateTicketStatusRequest> updateStatusValidator,
        IValidator<EscalateTicketRequest> escalateValidator,
        IValidator<AddCommentRequest> commentValidator,
        IValidator<ConfirmAttachmentRequest> attachmentValidator)
    {
        _context = context;
        _s3StorageService = s3StorageService;
        _createValidator = createValidator;
        _updateStatusValidator = updateStatusValidator;
        _escalateValidator = escalateValidator;
        _commentValidator = commentValidator;
        _attachmentValidator = attachmentValidator;
    }

    public async Task<List<TicketSummaryDto>> GetTicketsAsync(TicketFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Include(t => t.Cliente)
            .Include(t => t.Prioridad)
            .Include(t => t.Estado)
            .Include(t => t.Categoria)
            .Include(t => t.AsignadoA)
            .AsQueryable();

        if (filter.EstadoId.HasValue)
            query = query.Where(t => t.EstadoId == filter.EstadoId.Value);

        if (filter.PrioridadId.HasValue)
            query = query.Where(t => t.PrioridadId == filter.PrioridadId.Value);

        if (filter.CategoriaId.HasValue)
            query = query.Where(t => t.CategoriaId == filter.CategoriaId.Value);

        if (filter.AsignadoAId.HasValue)
            query = query.Where(t => t.AsignadoAId == filter.AsignadoAId.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(t =>
                t.Codigo.ToLower().Contains(term) ||
                t.Titulo.ToLower().Contains(term) ||
                t.Cliente.RazonSocial.ToLower().Contains(term));
        }

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketSummaryDto
            {
                Id = t.Id,
                Codigo = t.Codigo,
                Titulo = t.Titulo,
                ClienteRazonSocial = t.Cliente.RazonSocial,
                PrioridadCodigo = t.Prioridad.Codigo,
                PrioridadNombre = t.Prioridad.Nombre,
                PrioridadColorHex = t.Prioridad.ColorHex,
                EstadoCodigo = t.Estado.Codigo,
                EstadoNombre = t.Estado.Nombre,
                EstadoColorHex = t.Estado.ColorHex,
                CategoriaNombre = t.Categoria.Nombre,
                AsignadoANombre = t.AsignadoA != null ? $"{t.AsignadoA.Nombre} {t.AsignadoA.Apellido}" : null,
                Version = t.Version,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<TicketDto> GetTicketByIdAsync(int id, CancellationToken ct = default)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Cliente)
            .Include(t => t.ReportadoPor)
            .Include(t => t.AsignadoA)
            .Include(t => t.Categoria)
            .Include(t => t.Prioridad)
            .Include(t => t.Estado)
            .Include(t => t.Comentarios)
                .ThenInclude(c => c.Usuario)
            .Include(t => t.Adjuntos)
            .Include(t => t.Escalaciones)
                .ThenInclude(e => e.EscaladoPor)
            .Include(t => t.Escalaciones)
                .ThenInclude(e => e.AsignadoA)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (ticket == null)
            throw new NotFoundException("Ticket", id);

        return MapToTicketDto(ticket);
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketRequest request, int reportadoPorId, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var clienteExists = await _context.ClientesSaas.AnyAsync(c => c.Id == request.ClienteId, ct);
        if (!clienteExists)
            throw new NotFoundException("ClienteSaas", request.ClienteId);

        var categoriaExists = await _context.CategoriasIncidencia.AnyAsync(c => c.Id == request.CategoriaId && c.IsActive, ct);
        if (!categoriaExists)
            throw new NotFoundException("CategoriaIncidencia", request.CategoriaId);

        var prioridadExists = await _context.Prioridades.AnyAsync(p => p.Id == request.PrioridadId, ct);
        if (!prioridadExists)
            throw new NotFoundException("Prioridad", request.PrioridadId);

        // Estado inicial por defecto: NUEVO
        var estadoNuevo = await _context.EstadosTicket
            .FirstOrDefaultAsync(e => e.Codigo == "NUEVO", ct)
            ?? await _context.EstadosTicket.OrderBy(e => e.Orden).FirstAsync(ct);

        // Generar correlativo: INC-2026-0001
        var totalTicketsYear = await _context.Tickets.CountAsync(ct);
        var correlativo = $"INC-{DateTime.UtcNow.Year}-{(totalTicketsYear + 1).ToString("D4")}";

        var ticket = new Ticket
        {
            Codigo = correlativo,
            ClienteId = request.ClienteId,
            ReportadoPorId = reportadoPorId,
            CategoriaId = request.CategoriaId,
            PrioridadId = request.PrioridadId,
            EstadoId = estadoNuevo.Id,
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            TelefonoContacto = request.TelefonoContacto?.Trim(),
            CanalOrigen = request.CanalOrigen.Trim().ToUpperInvariant(),
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);

        return await GetTicketByIdAsync(ticket.Id, ct);
    }

    public async Task<TicketDto> UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request, int usuarioId, CancellationToken ct = default)
    {
        var validation = await _updateStatusValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (ticket == null)
            throw new NotFoundException("Ticket", id);

        // BLOQUEO OPTIMISTA: Verificación previa en memoria
        if (ticket.Version != request.CurrentVersion)
        {
            throw new ConflictException(
                $"El ticket fue modificado por otro operador en simultáneo (Versión recibida: {request.CurrentVersion}, versión actual en base de datos: {ticket.Version}). Por favor recargue la pantalla antes de continuar.");
        }

        var nuevoEstado = await _context.EstadosTicket.FirstOrDefaultAsync(e => e.Id == request.NuevoEstadoId, ct);
        if (nuevoEstado == null)
            throw new NotFoundException("EstadoTicket", request.NuevoEstadoId);

        ticket.EstadoId = nuevoEstado.Id;
        ticket.Version += 1; // Incremento de versión
        ticket.UpdatedAt = DateTime.UtcNow;

        if (nuevoEstado.EsFinal)
            ticket.FechaCierre = DateTime.UtcNow;
        else
            ticket.FechaCierre = null;

        if (!string.IsNullOrWhiteSpace(request.Comentario))
        {
            _context.TicketComentarios.Add(new TicketComentario
            {
                TicketId = ticket.Id,
                UsuarioId = usuarioId,
                Comentario = request.Comentario.Trim(),
                EsPrivadoTi = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Conflicto de concurrencia al actualizar el ticket. Otro usuario confirmó cambios primero.");
        }

        return await GetTicketByIdAsync(ticket.Id, ct);
    }

    public async Task<TicketDto> EscalateTicketAsync(int id, EscalateTicketRequest request, int escaladoPorId, CancellationToken ct = default)
    {
        var validation = await _escalateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (ticket == null)
            throw new NotFoundException("Ticket", id);

        if (ticket.Version != request.CurrentVersion)
        {
            throw new ConflictException("El ticket fue modificado recientemente. Recargue antes de escalar.");
        }

        var targetUser = await _context.UsuariosInternos
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == request.AsignadoAId && u.IsActive, ct);

        if (targetUser == null)
            throw new NotFoundException("Usuario", request.AsignadoAId);

        var escalacion = new EscalacionTicket
        {
            TicketId = ticket.Id,
            EscaladoPorId = escaladoPorId,
            AsignadoAId = targetUser.Id,
            MotivoEscalacion = request.MotivoEscalacion.Trim(),
            DiagnosticoInicial = request.DiagnosticoInicial?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.EscalacionesTicket.Add(escalacion);

        ticket.AsignadoAId = targetUser.Id;
        ticket.Version += 1;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Si está en 'NUEVO', avanzar automáticamente a 'EN_DIAGNOSTICO' o 'EN_DESARROLLO'
        var estadoDiagnostico = await _context.EstadosTicket
            .FirstOrDefaultAsync(e => e.Codigo == "EN_DIAGNOSTICO" || e.Codigo == "EN_DESARROLLO", ct);
        if (estadoDiagnostico != null)
            ticket.EstadoId = estadoDiagnostico.Id;

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Conflicto de concurrencia al escalar el ticket.");
        }

        return await GetTicketByIdAsync(ticket.Id, ct);
    }

    public async Task<TicketComentarioDto> AddCommentAsync(int ticketId, AddCommentRequest request, int usuarioId, CancellationToken ct = default)
    {
        var validation = await _commentValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == ticketId, ct);
        if (!ticketExists)
            throw new NotFoundException("Ticket", ticketId);

        var usuario = await _context.UsuariosInternos.FirstAsync(u => u.Id == usuarioId, ct);

        var comentario = new TicketComentario
        {
            TicketId = ticketId,
            UsuarioId = usuarioId,
            Comentario = request.Comentario.Trim(),
            EsPrivadoTi = request.EsPrivadoTi,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketComentarios.Add(comentario);
        await _context.SaveChangesAsync(ct);

        return new TicketComentarioDto
        {
            Id = comentario.Id,
            UsuarioId = usuario.Id,
            UsuarioNombre = $"{usuario.Nombre} {usuario.Apellido}",
            Comentario = comentario.Comentario,
            EsPrivadoTi = comentario.EsPrivadoTi,
            CreatedAt = comentario.CreatedAt
        };
    }

    public Task<PresignedUrlResponse> GetPresignedUploadUrlAsync(int ticketId, PresignedUrlRequest request, int usuarioId, CancellationToken ct = default)
    {
        var uploadUrl = _s3StorageService.GeneratePreSignedUploadUrl(
            request.FileName,
            request.ContentType,
            out var s3Key,
            expirationMinutes: 5);

        return Task.FromResult(new PresignedUrlResponse
        {
            UploadUrl = uploadUrl,
            S3Key = s3Key,
            ExpiresInMinutes = 5
        });
    }

    public async Task<TicketAdjuntoDto> ConfirmAttachmentAsync(int ticketId, ConfirmAttachmentRequest request, int usuarioId, CancellationToken ct = default)
    {
        var validation = await _attachmentValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == ticketId, ct);
        if (!ticketExists)
            throw new NotFoundException("Ticket", ticketId);

        var downloadUrl = _s3StorageService.GeneratePreSignedDownloadUrl(request.S3Key);

        var adjunto = new TicketAdjunto
        {
            TicketId = ticketId,
            UsuarioId = usuarioId,
            NombreArchivo = request.NombreArchivo.Trim(),
            S3Key = request.S3Key.Trim(),
            S3Url = downloadUrl,
            TipoMime = request.TipoMime.Trim(),
            TamanioBytes = request.TamanioBytes,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketAdjuntos.Add(adjunto);
        await _context.SaveChangesAsync(ct);

        return new TicketAdjuntoDto
        {
            Id = adjunto.Id,
            NombreArchivo = adjunto.NombreArchivo,
            S3Url = adjunto.S3Url,
            TipoMime = adjunto.TipoMime,
            TamanioBytes = adjunto.TamanioBytes,
            CreatedAt = adjunto.CreatedAt
        };
    }

    public async Task<List<CatalogItemDto>> GetPrioridadesAsync(CancellationToken ct = default)
    {
        return await _context.Prioridades
            .AsNoTracking()
            .OrderBy(p => p.Orden)
            .Select(p => new CatalogItemDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                ColorHex = p.ColorHex,
                Descripcion = $"SLA Resolución: {p.TiempoMaxResolucionHoras} hrs"
            })
            .ToListAsync(ct);
    }

    public async Task<List<CatalogItemDto>> GetEstadosAsync(CancellationToken ct = default)
    {
        return await _context.EstadosTicket
            .AsNoTracking()
            .OrderBy(e => e.Orden)
            .Select(e => new CatalogItemDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Nombre = e.Nombre,
                ColorHex = e.ColorHex
            })
            .ToListAsync(ct);
    }

    public async Task<List<CatalogItemDto>> GetCategoriasAsync(CancellationToken ct = default)
    {
        return await _context.CategoriasIncidencia
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Nombre)
            .Select(c => new CatalogItemDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            })
            .ToListAsync(ct);
    }

    private static TicketDto MapToTicketDto(Ticket ticket)
    {
        return new TicketDto
        {
            Id = ticket.Id,
            Codigo = ticket.Codigo,
            Titulo = ticket.Titulo,
            Descripcion = ticket.Descripcion,
            TelefonoContacto = ticket.TelefonoContacto,
            CanalOrigen = ticket.CanalOrigen,
            Version = ticket.Version,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            FechaCierre = ticket.FechaCierre,

            ClienteId = ticket.ClienteId,
            ClienteRazonSocial = ticket.Cliente?.RazonSocial ?? string.Empty,
            ClienteRuc = ticket.Cliente?.Ruc ?? string.Empty,

            ReportadoPorId = ticket.ReportadoPorId,
            ReportadoPorNombre = ticket.ReportadoPor != null ? $"{ticket.ReportadoPor.Nombre} {ticket.ReportadoPor.Apellido}" : string.Empty,

            AsignadoAId = ticket.AsignadoAId,
            AsignadoANombre = ticket.AsignadoA != null ? $"{ticket.AsignadoA.Nombre} {ticket.AsignadoA.Apellido}" : null,

            CategoriaId = ticket.CategoriaId,
            CategoriaCodigo = ticket.Categoria?.Codigo ?? string.Empty,
            CategoriaNombre = ticket.Categoria?.Nombre ?? string.Empty,

            PrioridadId = ticket.PrioridadId,
            PrioridadCodigo = ticket.Prioridad?.Codigo ?? string.Empty,
            PrioridadNombre = ticket.Prioridad?.Nombre ?? string.Empty,
            PrioridadColorHex = ticket.Prioridad?.ColorHex ?? string.Empty,
            TiempoMaxResolucionHoras = ticket.Prioridad?.TiempoMaxResolucionHoras ?? 0,

            EstadoId = ticket.EstadoId,
            EstadoCodigo = ticket.Estado?.Codigo ?? string.Empty,
            EstadoNombre = ticket.Estado?.Nombre ?? string.Empty,
            EstadoColorHex = ticket.Estado?.ColorHex ?? string.Empty,
            EsFinal = ticket.Estado?.EsFinal ?? false,

            Comentarios = ticket.Comentarios.Select(c => new TicketComentarioDto
            {
                Id = c.Id,
                UsuarioId = c.UsuarioId,
                UsuarioNombre = c.Usuario != null ? $"{c.Usuario.Nombre} {c.Usuario.Apellido}" : string.Empty,
                Comentario = c.Comentario,
                EsPrivadoTi = c.EsPrivadoTi,
                CreatedAt = c.CreatedAt
            }).OrderBy(c => c.CreatedAt).ToList(),

            Adjuntos = ticket.Adjuntos.Select(a => new TicketAdjuntoDto
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                S3Url = a.S3Url,
                TipoMime = a.TipoMime,
                TamanioBytes = a.TamanioBytes,
                CreatedAt = a.CreatedAt
            }).ToList(),

            Escalaciones = ticket.Escalaciones.Select(e => new EscalacionDto
            {
                Id = e.Id,
                EscaladoPorNombre = e.EscaladoPor != null ? $"{e.EscaladoPor.Nombre} {e.EscaladoPor.Apellido}" : string.Empty,
                AsignadoANombre = e.AsignadoA != null ? $"{e.AsignadoA.Nombre} {e.AsignadoA.Apellido}" : string.Empty,
                MotivoEscalacion = e.MotivoEscalacion,
                DiagnosticoInicial = e.DiagnosticoInicial,
                CreatedAt = e.CreatedAt
            }).OrderBy(e => e.CreatedAt).ToList()
        };
    }
}
