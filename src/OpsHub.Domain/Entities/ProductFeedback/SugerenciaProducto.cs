using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.ProductFeedback;

public class SugerenciaProducto : BaseEntity, IAuditableEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ModuloAfectado { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public int CreadoPorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public EstadoSugerencia Estado { get; set; } = null!;
    public UsuarioInterno CreadoPor { get; set; } = null!;

    public ICollection<VotoSugerencia> Votos { get; set; } = new List<VotoSugerencia>();
    public ICollection<ClienteSolicitante> ClientesSolicitantes { get; set; } = new List<ClienteSolicitante>();
    public PriorizacionSugerencia? Priorizacion { get; set; }
}
