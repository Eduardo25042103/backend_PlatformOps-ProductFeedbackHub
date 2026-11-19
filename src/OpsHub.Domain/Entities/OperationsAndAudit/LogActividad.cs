using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Domain.Entities.OperationsAndAudit;

public class LogActividad
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string TablaAfectada { get; set; } = string.Empty;
    public int RegistroId { get; set; }
    public string? IpOrigen { get; set; }
    public string? Plataforma { get; set; }
    public string? ValoresAnterioresJson { get; set; }
    public string? ValoresNuevosJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UsuarioInterno? Usuario { get; set; }
}
