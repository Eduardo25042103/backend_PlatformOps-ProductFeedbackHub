using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Domain.Entities.OperationsAndAudit;

public class MetricaTicket : BaseEntity
{
    public int TicketId { get; set; }
    public int? MinutosPrimeraRespuesta { get; set; }
    public int? MinutosResolucion { get; set; }
    public bool? CumplioSlaRespuesta { get; set; }
    public bool? CumplioSlaResolucion { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
