using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.OperationsAndAudit;

public class NotificacionUsuario : BaseEntity
{
    public int UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public bool Leido { get; set; }
    public string? UrlDestino { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UsuarioInterno Usuario { get; set; } = null!;
}
