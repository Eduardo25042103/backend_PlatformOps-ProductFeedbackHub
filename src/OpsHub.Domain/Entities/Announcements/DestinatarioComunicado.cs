using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;
using OpsHub.Domain.Entities.Commercial;

namespace OpsHub.Domain.Entities.Announcements;

public class DestinatarioComunicado : BaseEntity
{
    public int ComunicadoId { get; set; }
    public int? DepartamentoId { get; set; }
    public int? ClienteId { get; set; }

    public ComunicadoSistema Comunicado { get; set; } = null!;
    public Departamento? Departamento { get; set; }
    public ClienteSaas? Cliente { get; set; }
}
