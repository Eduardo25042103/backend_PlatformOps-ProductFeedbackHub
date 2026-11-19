using OpsHub.Domain.Common;
using OpsHub.Domain.Entities.AccessControl;
using OpsHub.Domain.Entities.Commercial;
using OpsHub.Domain.Entities.OperationsAndAudit;

namespace OpsHub.Domain.Entities.Incidents;

public class Ticket : BaseEntity, IAuditableEntity
{
    public string Codigo { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int ReportadoPorId { get; set; }
    public int? AsignadoAId { get; set; }
    public int CategoriaId { get; set; }
    public int PrioridadId { get; set; }
    public int EstadoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? TelefonoContacto { get; set; }
    public string CanalOrigen { get; set; } = "WEB";
    
    // Control de Concurrencia Optimista (HTTP 409 Conflict)
    public int Version { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? FechaCierre { get; set; }

    public ClienteSaas Cliente { get; set; } = null!;
    public UsuarioInterno ReportadoPor { get; set; } = null!;
    public UsuarioInterno? AsignadoA { get; set; }
    public CategoriaIncidencia Categoria { get; set; } = null!;
    public Prioridad Prioridad { get; set; } = null!;
    public EstadoTicket Estado { get; set; } = null!;

    public MetricaTicket? Metrica { get; set; }
    public EvaluacionAtencion? Evaluacion { get; set; }

    public ICollection<TicketComentario> Comentarios { get; set; } = new List<TicketComentario>();
    public ICollection<TicketAdjunto> Adjuntos { get; set; } = new List<TicketAdjunto>();
    public ICollection<EscalacionTicket> Escalaciones { get; set; } = new List<EscalacionTicket>();
}
