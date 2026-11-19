using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.Commercial;

public class NotaCliente : BaseEntity
{
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public string TipoNota { get; set; } = "GENERAL"; // SOPORTE, COMERCIAL, COBRANZA, GENERAL
    public string Nota { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClienteSaas Cliente { get; set; } = null!;
    public UsuarioInterno Usuario { get; set; } = null!;
}
