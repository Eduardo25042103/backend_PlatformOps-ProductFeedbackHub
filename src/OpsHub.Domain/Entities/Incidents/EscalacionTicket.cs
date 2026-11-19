using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.Incidents;

public class EscalacionTicket : BaseEntity
{
    public int TicketId { get; set; }
    public int EscaladoPorId { get; set; }
    public int AsignadoAId { get; set; }
    public string MotivoEscalacion { get; set; } = string.Empty;
    public string? DiagnosticoInicial { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
    public UsuarioInterno EscaladoPor { get; set; } = null!;
    public UsuarioInterno AsignadoA { get; set; } = null!;
}
