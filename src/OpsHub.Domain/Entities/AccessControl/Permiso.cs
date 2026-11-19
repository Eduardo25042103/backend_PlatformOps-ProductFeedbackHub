using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.AccessControl;

public class Permiso : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RolPermiso> RolesPermisos { get; set; } = new List<RolPermiso>();
}
