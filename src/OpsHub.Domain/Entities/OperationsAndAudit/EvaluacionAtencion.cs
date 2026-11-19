using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Domain.Entities.OperationsAndAudit;

public class EvaluacionAtencion : BaseEntity
{
    public int TicketId { get; set; }
    public int PuntajeEstrellas { get; set; }
    public string? Comentarios { get; set; }
    public DateTime EvaluadoEn { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
}
