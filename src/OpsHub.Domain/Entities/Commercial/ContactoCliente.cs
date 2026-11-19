using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.Commercial;

public class ContactoCliente : BaseEntity
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool EsPrincipal { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClienteSaas Cliente { get; set; } = null!;
}
