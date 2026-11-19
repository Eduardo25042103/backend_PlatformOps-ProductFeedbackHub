using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Exceptions;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Features.Operations.DTOs;
using OpsHub.Domain.Entities.AccessControl;
using OpsHub.Domain.Entities.Incidents;
using OpsHub.Domain.Entities.OperationsAndAudit;
using ValidationException = OpsHub.Application.Common.Exceptions.ValidationException;

namespace OpsHub.Application.Features.Operations.Services;

public class OperationsService : IOperationsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateEvaluacionRequest> _evaluacionValidator;
    private readonly IValidator<CreateTurnoRequest> _turnoValidator;
    private readonly IValidator<CreateNotificacionRequest> _notificacionValidator;

    public OperationsService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IValidator<CreateEvaluacionRequest> evaluacionValidator,
        IValidator<CreateTurnoRequest> turnoValidator,
        IValidator<CreateNotificacionRequest> notificacionValidator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _evaluacionValidator = evaluacionValidator;
        _turnoValidator = turnoValidator;
        _notificacionValidator = notificacionValidator;
    }

    public async Task<List<LogActividadDto>> GetAuditLogsAsync(
        AuditFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LogsActividad
            .AsNoTracking()
            .Include(l => l.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.TablaAfectada))
            query = query.Where(l => l.TablaAfectada == filter.TablaAfectada);

        if (!string.IsNullOrWhiteSpace(filter.Accion))
            query = query.Where(l => l.Accion == filter.Accion);

        if (filter.UsuarioId.HasValue)
            query = query.Where(l => l.UsuarioId == filter.UsuarioId.Value);

        var limit = filter.Limit > 0 && filter.Limit <= 100 ? filter.Limit : 50;

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return logs.Select(l => new LogActividadDto(
            l.Id,
            l.UsuarioId,
            l.Usuario != null ? $"{l.Usuario.Nombre} {l.Usuario.Apellido}".Trim() : null,
            l.Usuario?.Email,
            l.Accion,
            l.TablaAfectada,
            l.RegistroId,
            l.IpOrigen,
            l.Plataforma,
            l.ValoresAnterioresJson,
            l.ValoresNuevosJson,
            l.CreatedAt
        )).ToList();
    }

    public async Task<MetricaSlaSummaryDto> GetSlaMetricsSummaryAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.MetricasTickets.CountAsync(cancellationToken);
        if (total == 0)
        {
            return new MetricaSlaSummaryDto(0, 0, 0, 100.0, 100.0, 0, 0, new List<PlanSlaBreakdownDto>());
        }

        var cumplieronRespuesta = await _context.MetricasTickets
            .CountAsync(m => m.CumplioSlaRespuesta == true, cancellationToken);

        var cumplieronResolucion = await _context.MetricasTickets
            .CountAsync(m => m.CumplioSlaResolucion == true, cancellationToken);

        var avgRespuesta = await _context.MetricasTickets
            .Where(m => m.MinutosPrimeraRespuesta.HasValue)
            .AverageAsync(m => (double?)m.MinutosPrimeraRespuesta, cancellationToken) ?? 0.0;

        var avgResolucion = await _context.MetricasTickets
            .Where(m => m.MinutosResolucion.HasValue)
            .AverageAsync(m => (double?)m.MinutosResolucion, cancellationToken) ?? 0.0;

        var pctRespuesta = Math.Round((double)cumplieronRespuesta / total * 100.0, 2);
        var pctResolucion = Math.Round((double)cumplieronResolucion / total * 100.0, 2);

        // Desglose dinámico por plan de suscripción SaaS
        var metricasConPlan = await _context.MetricasTickets
            .AsNoTracking()
            .Include(m => m.Ticket)
                .ThenInclude(t => t.Cliente)
                    .ThenInclude(c => c.Plan)
            .ToListAsync(cancellationToken);

        var planes = await _context.PlanesSuscripcion.AsNoTracking().ToListAsync(cancellationToken);
        var desglose = planes.Select(p =>
        {
            var ticketsDelPlan = metricasConPlan.Where(m => m.Ticket?.Cliente?.PlanId == p.Id).ToList();
            var totalPlan = ticketsDelPlan.Count;
            var enSlaPlan = ticketsDelPlan.Count(m => m.CumplioSlaRespuesta == true);
            var fueraSlaPlan = totalPlan - enSlaPlan;
            var pctPlan = totalPlan > 0 ? Math.Round((double)enSlaPlan / totalPlan * 100.0, 1) : 100.0;
            var mttrMinutos = ticketsDelPlan.Where(m => m.MinutosResolucion.HasValue).Select(m => (double)m.MinutosResolucion!.Value).DefaultIfEmpty(0).Average();
            var mttrHoras = Math.Round(mttrMinutos / 60.0, 1);

            return new PlanSlaBreakdownDto(
                p.Nombre,
                p.Codigo,
                totalPlan,
                enSlaPlan,
                fueraSlaPlan,
                pctPlan,
                p.SlaHorasResolucion,
                mttrHoras
            );
        }).ToList();

        return new MetricaSlaSummaryDto(
            total,
            cumplieronRespuesta,
            cumplieronResolucion,
            pctRespuesta,
            pctResolucion,
            Math.Round(avgRespuesta, 1),
            Math.Round(avgResolucion, 1),
            desglose
        );
    }

    public async Task<EvaluacionAtencionDto> SubmitEvaluationAsync(
        CreateEvaluacionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _evaluacionValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var ticket = await _context.Tickets
            .Include(t => t.Evaluacion)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
            throw new NotFoundException(nameof(Ticket), request.TicketId);

        if (ticket.Evaluacion != null)
            throw new ConflictException($"El ticket {ticket.Codigo} ya cuenta con una evaluación de satisfacción registrada.");

        var evaluacion = new EvaluacionAtencion
        {
            TicketId = request.TicketId,
            PuntajeEstrellas = request.PuntajeEstrellas,
            Comentarios = request.Comentarios,
            EvaluadoEn = DateTime.UtcNow
        };

        _context.EvaluacionesAtencion.Add(evaluacion);
        await _context.SaveChangesAsync(cancellationToken);

        return new EvaluacionAtencionDto(
            evaluacion.Id,
            evaluacion.TicketId,
            ticket.Codigo,
            evaluacion.PuntajeEstrellas,
            evaluacion.Comentarios,
            evaluacion.EvaluadoEn
        );
    }

    public async Task<List<NotificacionDto>> GetUserNotificationsAsync(
        bool? onlyUnread = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            var firstUser = await _context.UsuariosInternos.FirstOrDefaultAsync(cancellationToken);
            userId = firstUser?.Id ?? 1;
        }

        var query = _context.NotificacionesUsuario
            .AsNoTracking()
            .Where(n => n.UsuarioId == userId.Value);

        if (onlyUnread == true)
            query = query.Where(n => !n.Leido);

        var list = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return list.Select(n => new NotificacionDto(
            n.Id,
            n.UsuarioId,
            n.Titulo,
            n.Mensaje,
            n.TipoEvento,
            n.Leido,
            n.UrlDestino,
            n.CreatedAt
        )).ToList();
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var notif = await _context.NotificacionesUsuario.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notif == null) return false;

        notif.Leido = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<NotificacionDto> CreateNotificationAsync(
        CreateNotificacionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _notificacionValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var notif = new NotificacionUsuario
        {
            UsuarioId = request.UsuarioId,
            Titulo = request.Titulo,
            Mensaje = request.Mensaje,
            TipoEvento = request.TipoEvento,
            UrlDestino = request.UrlDestino,
            Leido = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.NotificacionesUsuario.Add(notif);
        await _context.SaveChangesAsync(cancellationToken);

        return new NotificacionDto(
            notif.Id,
            notif.UsuarioId,
            notif.Titulo,
            notif.Mensaje,
            notif.TipoEvento,
            notif.Leido,
            notif.UrlDestino,
            notif.CreatedAt
        );
    }

    public async Task<List<TurnoDto>> GetActiveShiftsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var shifts = await _context.TurnosDisponibilidad
            .AsNoTracking()
            .Include(t => t.Usuario)
            .Where(t => t.EstaActivo && t.FechaFin >= now)
            .OrderBy(t => t.FechaInicio)
            .ToListAsync(cancellationToken);

        return shifts.Select(t => new TurnoDto(
            t.Id,
            t.UsuarioId,
            $"{t.Usuario.Nombre} {t.Usuario.Apellido}".Trim(),
            t.FechaInicio,
            t.FechaFin,
            t.EstaActivo
        )).ToList();
    }

    public async Task<TurnoDto> CreateShiftAsync(
        CreateTurnoRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _turnoValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var usuario = await _context.UsuariosInternos.FindAsync(new object[] { request.UsuarioId }, cancellationToken);
        if (usuario == null)
            throw new NotFoundException(nameof(UsuarioInterno), request.UsuarioId);

        var turno = new TurnoDisponibilidad
        {
            UsuarioId = request.UsuarioId,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            EstaActivo = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.TurnosDisponibilidad.Add(turno);
        await _context.SaveChangesAsync(cancellationToken);

        return new TurnoDto(
            turno.Id,
            turno.UsuarioId,
            $"{usuario.Nombre} {usuario.Apellido}".Trim(),
            turno.FechaInicio,
            turno.FechaFin,
            turno.EstaActivo
        );
    }
}
