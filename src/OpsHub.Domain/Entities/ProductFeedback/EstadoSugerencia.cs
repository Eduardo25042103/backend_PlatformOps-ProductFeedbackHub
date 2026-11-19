using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.ProductFeedback;

public class EstadoSugerencia : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; } = 1;

    public ICollection<SugerenciaProducto> Sugerencias { get; set; } = new List<SugerenciaProducto>();
}
