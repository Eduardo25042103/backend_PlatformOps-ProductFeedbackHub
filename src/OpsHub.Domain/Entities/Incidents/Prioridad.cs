using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.Incidents;

public class Prioridad : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int TiempoMaxRespuestaMin { get; set; }
    public int TiempoMaxResolucionHoras { get; set; }
    public string ColorHex { get; set; } = "#6366F1";
    public int Orden { get; set; } = 1;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
