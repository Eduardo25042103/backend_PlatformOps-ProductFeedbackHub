using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.ProductFeedback;

public class VotoSugerencia : BaseEntity
{
    public int SugerenciaId { get; set; }
    public int UsuarioId { get; set; }
    public string? MotivoVoto { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SugerenciaProducto Sugerencia { get; set; } = null!;
    public UsuarioInterno Usuario { get; set; } = null!;
}
