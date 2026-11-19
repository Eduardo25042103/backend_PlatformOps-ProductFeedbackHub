using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.Announcements;

public class AnuncioChangelog : BaseEntity
{
    public string VersionTag { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;
    public int RedactadoPorId { get; set; }
    public DateOnly FechaLanzamiento { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UsuarioInterno RedactadoPor { get; set; } = null!;
}
