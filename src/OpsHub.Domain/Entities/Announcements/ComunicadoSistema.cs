using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.Announcements;

public class ComunicadoSistema : BaseEntity, IAuditableEntity
{
    public int TipoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool AfectaServicio { get; set; }
    public int PublicadoPorId { get; set; }
    public DateTime? InicioEvento { get; set; }
    public DateTime? FinEstimado { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public TipoComunicado Tipo { get; set; } = null!;
    public UsuarioInterno PublicadoPor { get; set; } = null!;
    public ICollection<DestinatarioComunicado> Destinatarios { get; set; } = new List<DestinatarioComunicado>();
}
