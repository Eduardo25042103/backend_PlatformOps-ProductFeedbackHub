using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsHub.Application.Common.Models;
using OpsHub.Application.Features.Operations.DTOs;
using OpsHub.Application.Features.Operations.Services;

namespace OpsHub.Api.Controllers;

[Route("api/v1/operations")]
public class OperationsController : BaseApiController
{
    private readonly IOperationsService _operationsService;

    public OperationsController(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    /// <summary>
    /// Consulta los logs de auditoría técnica y transaccional generados automáticamente.
    /// </summary>
    [HttpGet("audit")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<LogActividadDto>>>> GetAuditLogs(
        [FromQuery] string? tabla = null,
        [FromQuery] string? accion = null,
        [FromQuery] int? usuarioId = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var filter = new AuditFilterDto(tabla, accion, usuarioId, limit);
        var data = await _operationsService.GetAuditLogsAsync(filter, cancellationToken);
        return Ok(ApiResponse<List<LogActividadDto>>.Ok(data));
    }

    /// <summary>
    /// Retorna el resumen consolidado de cumplimiento de SLAs de primera respuesta y resolución.
    /// </summary>
    [HttpGet("metrics/sla")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MetricaSlaSummaryDto>>> GetSlaMetrics(
        CancellationToken cancellationToken = default)
    {
        var data = await _operationsService.GetSlaMetricsSummaryAsync(cancellationToken);
        return Ok(ApiResponse<MetricaSlaSummaryDto>.Ok(data));
    }

    /// <summary>
    /// Registra la evaluación de satisfacción del cliente o usuario sobre un ticket atendido (1 a 5 estrellas).
    /// </summary>
    [HttpPost("evaluations")]
    public async Task<ActionResult<ApiResponse<EvaluacionAtencionDto>>> SubmitEvaluation(
        [FromBody] CreateEvaluacionRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _operationsService.SubmitEvaluationAsync(request, cancellationToken);
        return Ok(ApiResponse<EvaluacionAtencionDto>.Ok(data, "Evaluación de atención registrada con éxito."));
    }

    /// <summary>
    /// Lista las notificaciones de la bandeja del usuario autenticado.
    /// </summary>
    [HttpGet("notifications")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> GetNotifications(
        [FromQuery] bool? onlyUnread = null,
        CancellationToken cancellationToken = default)
    {
        var data = await _operationsService.GetUserNotificationsAsync(onlyUnread, cancellationToken);
        return Ok(ApiResponse<List<NotificacionDto>>.Ok(data));
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    [HttpPut("notifications/{id:int}/read")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> MarkNotificationAsRead(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var success = await _operationsService.MarkNotificationAsReadAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(success, success ? "Notificación marcada como leída." : "Notificación no encontrada."));
    }

    /// <summary>
    /// Emite una nueva notificación dirigida a un usuario interno.
    /// </summary>
    [HttpPost("notifications")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<NotificacionDto>>> CreateNotification(
        [FromBody] CreateNotificacionRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _operationsService.CreateNotificationAsync(request, cancellationToken);
        return Ok(ApiResponse<NotificacionDto>.Ok(data, "Notificación emitida con éxito."));
    }

    /// <summary>
    /// Consulta la lista de turnos de soporte y guardia técnica vigentes.
    /// </summary>
    [HttpGet("shifts")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<TurnoDto>>>> GetActiveShifts(
        CancellationToken cancellationToken = default)
    {
        var data = await _operationsService.GetActiveShiftsAsync(cancellationToken);
        return Ok(ApiResponse<List<TurnoDto>>.Ok(data));
    }

    /// <summary>
    /// Asigna una ventana de turno u horario de guardia para un miembro del equipo de soporte/TI.
    /// </summary>
    [HttpPost("shifts")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TurnoDto>>> CreateShift(
        [FromBody] CreateTurnoRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _operationsService.CreateShiftAsync(request, cancellationToken);
        return Ok(ApiResponse<TurnoDto>.Ok(data, "Turno registrado exitosamente."));
    }
}
