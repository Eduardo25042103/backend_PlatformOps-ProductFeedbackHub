using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.AccessControl;

public class UsuarioInterno : BaseEntity, IAuditableEntity
{
    public string? CognitoSub { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int DepartamentoId { get; set; }
    public int RolId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UltimoLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Departamento Departamento { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}
