using OpsHub.Application.Features.Operations.DTOs;

namespace OpsHub.Application.Features.Operations.Services;

public interface IOperationsService
{
    Task<List<LogActividadDto>> GetAuditLogsAsync(AuditFilterDto filter, CancellationToken cancellationToken = default);
    Task<MetricaSlaSummaryDto> GetSlaMetricsSummaryAsync(CancellationToken cancellationToken = default);
    Task<EvaluacionAtencionDto> SubmitEvaluationAsync(CreateEvaluacionRequest request, CancellationToken cancellationToken = default);
    Task<List<NotificacionDto>> GetUserNotificationsAsync(bool? onlyUnread = null, CancellationToken cancellationToken = default);
    Task<bool> MarkNotificationAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task<NotificacionDto> CreateNotificationAsync(CreateNotificacionRequest request, CancellationToken cancellationToken = default);
    Task<List<TurnoDto>> GetActiveShiftsAsync(CancellationToken cancellationToken = default);
    Task<TurnoDto> CreateShiftAsync(CreateTurnoRequest request, CancellationToken cancellationToken = default);
}
