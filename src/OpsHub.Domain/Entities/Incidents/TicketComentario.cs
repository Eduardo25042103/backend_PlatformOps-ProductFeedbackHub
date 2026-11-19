using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.Incidents;

public class TicketComentario : BaseEntity
{
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public bool EsPrivadoTi { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
    public UsuarioInterno Usuario { get; set; } = null!;
}
