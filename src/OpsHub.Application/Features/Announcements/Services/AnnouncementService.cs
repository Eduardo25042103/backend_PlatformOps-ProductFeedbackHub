using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Exceptions;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Features.Announcements.DTOs;
using OpsHub.Domain.Entities.Announcements;
using ValidationException = OpsHub.Application.Common.Exceptions.ValidationException;

namespace OpsHub.Application.Features.Announcements.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateComunicadoRequest> _comunicadoValidator;
    private readonly IValidator<CreateChangelogRequest> _changelogValidator;

    public AnnouncementService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IValidator<CreateComunicadoRequest> comunicadoValidator,
        IValidator<CreateChangelogRequest> changelogValidator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _comunicadoValidator = comunicadoValidator;
        _changelogValidator = changelogValidator;
    }

    public async Task<List<ComunicadoDto>> GetActiveAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Comunicados activos: o afectan el servicio actualmente o fueron creados en las últimas 72 horas
        var thresholdDate = now.AddHours(-72);

        var query = _context.ComunicadosSistema
            .AsNoTracking()
            .Include(c => c.Tipo)
            .Include(c => c.PublicadoPor)
            .Include(c => c.Destinatarios)
                .ThenInclude(d => d.Departamento)
            .Include(c => c.Destinatarios)
                .ThenInclude(d => d.Cliente)
            .Where(c => c.AfectaServicio || c.CreatedAt >= thresholdDate)
            .OrderByDescending(c => c.AfectaServicio)
            .ThenByDescending(c => c.CreatedAt);

        var list = await query.ToListAsync(cancellationToken);
        return list.Select(MapToComunicadoDto).ToList();
    }

    public async Task<List<ComunicadoDto>> GetAnnouncementsAsync(
        int? tipoId = null,
        bool? afectaServicio = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ComunicadosSistema
            .AsNoTracking()
            .Include(c => c.Tipo)
            .Include(c => c.PublicadoPor)
            .Include(c => c.Destinatarios)
                .ThenInclude(d => d.Departamento)
            .Include(c => c.Destinatarios)
                .ThenInclude(d => d.Cliente)
            .AsQueryable();

        if (tipoId.HasValue)
            query = query.Where(c => c.TipoId == tipoId.Value);

        if (afectaServicio.HasValue)
            query = query.Where(c => c.AfectaServicio == afectaServicio.Value);

        query = query.OrderByDescending(c => c.CreatedAt);

        var list = await query.ToListAsync(cancellationToken);
        return list.Select(MapToComunicadoDto).ToList();
    }

    public async Task<ComunicadoDto> CreateAnnouncementAsync(
        CreateComunicadoRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _comunicadoValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var tipo = await _context.TiposComunicado.FindAsync(new object[] { request.TipoId }, cancellationToken);
        if (tipo == null)
            throw new NotFoundException(nameof(TipoComunicado), request.TipoId);

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            var firstUser = await _context.UsuariosInternos.FirstOrDefaultAsync(cancellationToken);
            userId = firstUser?.Id ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        }

        var comunicado = new ComunicadoSistema
        {
            TipoId = request.TipoId,
            Titulo = request.Titulo,
            Mensaje = request.Mensaje,
            AfectaServicio = request.AfectaServicio,
            PublicadoPorId = userId.Value,
            InicioEvento = request.InicioEvento,
            FinEstimado = request.FinEstimado,
            CreatedAt = DateTime.UtcNow
        };

        if (request.DepartamentoIds != null)
        {
            foreach (var deptoId in request.DepartamentoIds)
            {
                comunicado.Destinatarios.Add(new DestinatarioComunicado
                {
                    DepartamentoId = deptoId
                });
            }
        }

        if (request.ClienteIds != null)
        {
            foreach (var clienteId in request.ClienteIds)
            {
                comunicado.Destinatarios.Add(new DestinatarioComunicado
                {
                    ClienteId = clienteId
                });
            }
        }

        _context.ComunicadosSistema.Add(comunicado);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetComunicadoByIdAsync(comunicado.Id, cancellationToken);
    }

    public async Task<List<ChangelogDto>> GetChangelogsAsync(CancellationToken cancellationToken = default)
    {
        var changelogs = await _context.AnunciosChangelog
            .AsNoTracking()
            .Include(a => a.RedactadoPor)
            .OrderByDescending(a => a.FechaLanzamiento)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return changelogs.Select(a => new ChangelogDto(
            a.Id,
            a.VersionTag,
            a.Titulo,
            a.Resumen,
            a.RedactadoPorId,
            $"{a.RedactadoPor.Nombre} {a.RedactadoPor.Apellido}".Trim(),
            a.FechaLanzamiento,
            a.CreatedAt
        )).ToList();
    }

    public async Task<ChangelogDto> CreateChangelogAsync(
        CreateChangelogRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _changelogValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            var firstUser = await _context.UsuariosInternos.FirstOrDefaultAsync(cancellationToken);
            userId = firstUser?.Id ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        }

        var changelog = new AnuncioChangelog
        {
            VersionTag = request.VersionTag,
            Titulo = request.Titulo,
            Resumen = request.Resumen,
            RedactadoPorId = userId.Value,
            FechaLanzamiento = request.FechaLanzamiento,
            CreatedAt = DateTime.UtcNow
        };

        _context.AnunciosChangelog.Add(changelog);
        await _context.SaveChangesAsync(cancellationToken);

        var redactor = await _context.UsuariosInternos.FindAsync(new object[] { userId.Value }, cancellationToken);

        return new ChangelogDto(
            changelog.Id,
            changelog.VersionTag,
            changelog.Titulo,
            changelog.Resumen,
            changelog.RedactadoPorId,
            redactor != null ? $"{redactor.Nombre} {redactor.Apellido}".Trim() : "Desconocido",
            changelog.FechaLanzamiento,
            changelog.CreatedAt
        );
    }

    public async Task<List<TipoComunicadoDto>> GetTiposComunicadoAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TiposComunicado
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new TipoComunicadoDto(t.Id, t.Codigo, t.Nombre, t.ColorHex))
            .ToListAsync(cancellationToken);
    }

    private async Task<ComunicadoDto> GetComunicadoByIdAsync(int id, CancellationToken cancellationToken)
    {
        var comunicado = await _context.ComunicadosSistema
            .AsNoTracking()
            .Include(c => c.Tipo)
            .Include(c => c.PublicadoPor)
            .Include(c => c.Destinatarios)
                .ThenInclude(d => d.Departamento)
            .Include(c => c.Destinatarios)
                .ThenInclude(d => d.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (comunicado == null)
            throw new NotFoundException(nameof(ComunicadoSistema), id);

        return MapToComunicadoDto(comunicado);
    }

    private static ComunicadoDto MapToComunicadoDto(ComunicadoSistema c)
    {
        return new ComunicadoDto(
            c.Id,
            c.TipoId,
            c.Tipo.Codigo,
            c.Tipo.Nombre,
            c.Tipo.ColorHex,
            c.Titulo,
            c.Mensaje,
            c.AfectaServicio,
            c.PublicadoPorId,
            $"{c.PublicadoPor.Nombre} {c.PublicadoPor.Apellido}".Trim(),
            c.InicioEvento,
            c.FinEstimado,
            c.CreatedAt,
            c.Destinatarios.Select(d => new DestinatarioDto(
                d.Id,
                d.DepartamentoId,
                d.Departamento?.Nombre,
                d.ClienteId,
                d.Cliente?.RazonSocial
            )).ToList()
        );
    }
}
