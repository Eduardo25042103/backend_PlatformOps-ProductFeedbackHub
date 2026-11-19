using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.Commercial;

public class PlanSuscripcion : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioMensual { get; set; }
    public int LimiteUsuarios { get; set; } = 5;
    public int SlaHorasResolucion { get; set; } = 24;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ClienteSaas> Clientes { get; set; } = new List<ClienteSaas>();
}
