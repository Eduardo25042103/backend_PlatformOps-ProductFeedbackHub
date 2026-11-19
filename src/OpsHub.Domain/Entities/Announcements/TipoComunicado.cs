using OpsHub.Domain.Common;

namespace OpsHub.Domain.Entities.Announcements;

public class TipoComunicado : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#3B82F6";

    public ICollection<ComunicadoSistema> Comunicados { get; set; } = new List<ComunicadoSistema>();
}
