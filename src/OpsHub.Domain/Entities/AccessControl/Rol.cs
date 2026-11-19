using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.AccessControl;

public class Rol : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UsuarioInterno> Usuarios { get; set; } = new List<UsuarioInterno>();
    public ICollection<RolPermiso> RolesPermisos { get; set; } = new List<RolPermiso>();
}
