using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.Incidents;

public class TicketAdjunto : BaseEntity
{
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string S3Url { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamanioBytes { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
    public UsuarioInterno Usuario { get; set; } = null!;
}
