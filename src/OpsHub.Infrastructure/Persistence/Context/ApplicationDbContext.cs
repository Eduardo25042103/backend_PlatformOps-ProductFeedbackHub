using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Domain.Entities.AccessControl;
using OpsHub.Domain.Entities.Announcements;
using OpsHub.Domain.Entities.Commercial;
using OpsHub.Domain.Entities.Incidents;
using OpsHub.Domain.Entities.OperationsAndAudit;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Acceso y Organización (Módulo 1)
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
    public DbSet<UsuarioInterno> UsuariosInternos => Set<UsuarioInterno>();

    // Comercial y Clientes (Módulo 2)
    public DbSet<PlanSuscripcion> PlanesSuscripcion => Set<PlanSuscripcion>();
    public DbSet<ClienteSaas> ClientesSaas => Set<ClienteSaas>();
    public DbSet<ContactoCliente> ContactosCliente => Set<ContactoCliente>();
    public DbSet<NotaCliente> NotasCliente => Set<NotaCliente>();

    // Incidentes Técnicos - Core (Módulo 3)
    public DbSet<Prioridad> Prioridades => Set<Prioridad>();
    public DbSet<EstadoTicket> EstadosTicket => Set<EstadoTicket>();
    public DbSet<CategoriaIncidencia> CategoriasIncidencia => Set<CategoriaIncidencia>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComentario> TicketComentarios => Set<TicketComentario>();
    public DbSet<TicketAdjunto> TicketAdjuntos => Set<TicketAdjunto>();
    public DbSet<EscalacionTicket> EscalacionesTicket => Set<EscalacionTicket>();

    // Solicitudes de Producto y Feedback (Módulo 4)
    public DbSet<EstadoSugerencia> EstadosSugerencia => Set<EstadoSugerencia>();
    public DbSet<SugerenciaProducto> SugerenciasProducto => Set<SugerenciaProducto>();
    public DbSet<VotoSugerencia> VotosSugerencia => Set<VotoSugerencia>();
    public DbSet<ClienteSolicitante> ClientesSolicitantes => Set<ClienteSolicitante>();
    public DbSet<PriorizacionSugerencia> PriorizacionesSugerencia => Set<PriorizacionSugerencia>();

    // Comunicados Internos, Estado y Changelog (Módulo 5)
    public DbSet<TipoComunicado> TiposComunicado => Set<TipoComunicado>();
    public DbSet<ComunicadoSistema> ComunicadosSistema => Set<ComunicadoSistema>();
    public DbSet<DestinatarioComunicado> DestinatariosComunicado => Set<DestinatarioComunicado>();
    public DbSet<AnuncioChangelog> AnunciosChangelog => Set<AnuncioChangelog>();

    // SLAs, Auditoría, Turnos y Notificaciones (Módulo 6)
    public DbSet<MetricaTicket> MetricasTickets => Set<MetricaTicket>();
    public DbSet<EvaluacionAtencion> EvaluacionesAtencion => Set<EvaluacionAtencion>();
    public DbSet<TurnoDisponibilidad> TurnosDisponibilidad => Set<TurnoDisponibilidad>();
    public DbSet<LogActividad> LogsActividad => Set<LogActividad>();
    public DbSet<NotificacionUsuario> NotificacionesUsuario => Set<NotificacionUsuario>();

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return await Database.CanConnectAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
