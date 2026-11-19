using Microsoft.EntityFrameworkCore;
using OpsHub.Domain.Entities.AccessControl;
using OpsHub.Domain.Entities.Announcements;
using OpsHub.Domain.Entities.Commercial;
using OpsHub.Domain.Entities.Incidents;
using OpsHub.Domain.Entities.OperationsAndAudit;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Acceso y Organización (Módulo 1)
    DbSet<Departamento> Departamentos { get; }
    DbSet<Rol> Roles { get; }
    DbSet<Permiso> Permisos { get; }
    DbSet<RolPermiso> RolesPermisos { get; }
    DbSet<UsuarioInterno> UsuariosInternos { get; }

    // Comercial y Clientes (Módulo 2)
    DbSet<PlanSuscripcion> PlanesSuscripcion { get; }
    DbSet<ClienteSaas> ClientesSaas { get; }
    DbSet<ContactoCliente> ContactosCliente { get; }
    DbSet<NotaCliente> NotasCliente { get; }

    // Incidentes Técnicos - Core (Módulo 3)
    DbSet<Prioridad> Prioridades { get; }
    DbSet<EstadoTicket> EstadosTicket { get; }
    DbSet<CategoriaIncidencia> CategoriasIncidencia { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketComentario> TicketComentarios { get; }
    DbSet<TicketAdjunto> TicketAdjuntos { get; }
    DbSet<EscalacionTicket> EscalacionesTicket { get; }

    // Solicitudes de Producto y Feedback (Módulo 4)
    DbSet<EstadoSugerencia> EstadosSugerencia { get; }
    DbSet<SugerenciaProducto> SugerenciasProducto { get; }
    DbSet<VotoSugerencia> VotosSugerencia { get; }
    DbSet<ClienteSolicitante> ClientesSolicitantes { get; }
    DbSet<PriorizacionSugerencia> PriorizacionesSugerencia { get; }

    // Comunicados Internos, Estado y Changelog (Módulo 5)
    DbSet<TipoComunicado> TiposComunicado { get; }
    DbSet<ComunicadoSistema> ComunicadosSistema { get; }
    DbSet<DestinatarioComunicado> DestinatariosComunicado { get; }
    DbSet<AnuncioChangelog> AnunciosChangelog { get; }

    // SLAs, Auditoría, Turnos y Notificaciones (Módulo 6)
    DbSet<MetricaTicket> MetricasTickets { get; }
    DbSet<EvaluacionAtencion> EvaluacionesAtencion { get; }
    DbSet<TurnoDisponibilidad> TurnosDisponibilidad { get; }
    DbSet<LogActividad> LogsActividad { get; }
    DbSet<NotificacionUsuario> NotificacionesUsuario { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
