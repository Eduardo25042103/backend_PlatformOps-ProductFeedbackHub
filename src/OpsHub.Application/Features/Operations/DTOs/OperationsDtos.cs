namespace OpsHub.Application.Features.Operations.DTOs;

public record LogActividadDto(
    long Id,
    int? UsuarioId,
    string? UsuarioNombre,
    string? UsuarioEmail,
    string Accion,
    string TablaAfectada,
    int RegistroId,
    string? IpOrigen,
    string? Plataforma,
    string? ValoresAnterioresJson,
    string? ValoresNuevosJson,
    DateTime CreatedAt
);

public record AuditFilterDto(
    string? TablaAfectada = null,
    string? Accion = null,
    int? UsuarioId = null,
    int Limit = 50
);

public record PlanSlaBreakdownDto(
    string PlanNombre,
    string PlanCodigo,
    int TotalTickets,
    int TicketsEnSla,
    int TicketsFueraSla,
    double PorcentajeCumplimiento,
    int SlaHorasObjetivo,
    double MttrHorasPromedio
);

public record MetricaSlaSummaryDto(
    int TotalTicketsEvaluados,
    int CumplieronSlaRespuesta,
    int CumplieronSlaResolucion,
    double PorcentajeCumplimientoRespuesta,
    double PorcentajeCumplimientoResolucion,
    double PromedioMinutosPrimeraRespuesta,
    double PromedioMinutosResolucion,
    List<PlanSlaBreakdownDto>? DesglosePorPlan = null
);

public record CreateEvaluacionRequest(
    int TicketId,
    int PuntajeEstrellas,
    string? Comentarios
);

public record EvaluacionAtencionDto(
    int Id,
    int TicketId,
    string TicketCodigo,
    int PuntajeEstrellas,
    string? Comentarios,
    DateTime EvaluadoEn
);

public record NotificacionDto(
    int Id,
    int UsuarioId,
    string Titulo,
    string Mensaje,
    string TipoEvento,
    bool Leido,
    string? UrlDestino,
    DateTime CreatedAt
);

public record CreateNotificacionRequest(
    int UsuarioId,
    string Titulo,
    string Mensaje,
    string TipoEvento,
    string? UrlDestino
);

public record TurnoDto(
    int Id,
    int UsuarioId,
    string UsuarioNombre,
    DateTime FechaInicio,
    DateTime FechaFin,
    bool EstaActivo
);

public record CreateTurnoRequest(
    int UsuarioId,
    DateTime FechaInicio,
    DateTime FechaFin
);
