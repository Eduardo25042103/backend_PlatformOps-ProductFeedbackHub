using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.ProductFeedback;

public class PriorizacionSugerencia : BaseEntity
{
    public int SugerenciaId { get; set; }
    public int EsfuerzoTiDias { get; set; } = 1;
    public int ImpactoComercialScore { get; set; } = 1; // Escala 1 al 10
    public decimal PuntuacionFinal { get; set; } = 0.00m; // Fórmula ponderada
    public int EvaluadoPorId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public SugerenciaProducto Sugerencia { get; set; } = null!;
    public UsuarioInterno EvaluadoPor { get; set; } = null!;
}
