using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.Incidents;

public class EstadoTicket : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#64748B";
    public bool EsFinal { get; set; } = false;
    public int Orden { get; set; } = 1;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
