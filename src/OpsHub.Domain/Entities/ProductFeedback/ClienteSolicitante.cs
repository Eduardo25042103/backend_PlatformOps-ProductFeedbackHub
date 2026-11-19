using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.Commercial;

namespace OpsHub.Domain.Entities.ProductFeedback;

public class ClienteSolicitante : BaseEntity
{
    public int SugerenciaId { get; set; }
    public int ClienteId { get; set; }
    public decimal ValorAnualRepresentado { get; set; } = 0.00m;
    public string OrigenWebhook { get; set; } = "MANUAL"; // ZOHO_CRM, FORMULARIO, MANUAL
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SugerenciaProducto Sugerencia { get; set; } = null!;
    public ClienteSaas Cliente { get; set; } = null!;
}
