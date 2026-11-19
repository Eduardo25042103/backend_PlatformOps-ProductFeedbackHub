using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.OperationsAndAudit;

public class TurnoDisponibilidad : BaseEntity
{
    public int UsuarioId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool EstaActivo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UsuarioInterno Usuario { get; set; } = null!;
}
