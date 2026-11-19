using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.Incidents;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Domain.Entities.Commercial;

public class ClienteSaas : BaseEntity, IAuditableEntity
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public int PlanId { get; set; }
    public decimal MontoAnualContratado { get; set; }
    public string EstadoCuenta { get; set; } = "ACTIVO";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public PlanSuscripcion Plan { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<ContactoCliente> Contactos { get; set; } = new List<ContactoCliente>();
    public ICollection<NotaCliente> Notas { get; set; } = new List<NotaCliente>();
    public ICollection<ClienteSolicitante> SugerenciasSolicitadas { get; set; } = new List<ClienteSolicitante>();
}
