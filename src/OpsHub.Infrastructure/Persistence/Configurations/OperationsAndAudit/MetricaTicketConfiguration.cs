using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.OperationsAndAudit;

namespace OpsHub.Infrastructure.Persistence.Configurations.OperationsAndAudit;

public class MetricaTicketConfiguration : IEntityTypeConfiguration<MetricaTicket>
{
    public void Configure(EntityTypeBuilder<MetricaTicket> builder)
    {
        builder.ToTable("metricas_ticket");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.HasIndex(m => m.TicketId).IsUnique();

        builder.Property(m => m.MinutosPrimeraRespuesta).HasColumnName("minutos_primera_respuesta");
        builder.Property(m => m.MinutosResolucion).HasColumnName("minutos_resolucion");
        builder.Property(m => m.CumplioSlaRespuesta).HasColumnName("cumplio_sla_respuesta");
        builder.Property(m => m.CumplioSlaResolucion).HasColumnName("cumplio_sla_resolucion");

        builder.HasOne(m => m.Ticket)
            .WithOne(t => t.Metrica)
            .HasForeignKey<MetricaTicket>(m => m.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
