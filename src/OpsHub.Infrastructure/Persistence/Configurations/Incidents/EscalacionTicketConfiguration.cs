using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Infrastructure.Persistence.Configurations.Incidents;

public class EscalacionTicketConfiguration : IEntityTypeConfiguration<EscalacionTicket>
{
    public void Configure(EntityTypeBuilder<EscalacionTicket> builder)
    {
        builder.ToTable("escalaciones_ticket");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.Property(e => e.EscaladoPorId).HasColumnName("escalado_por_id").IsRequired();
        builder.Property(e => e.AsignadoAId).HasColumnName("asignado_a_id").IsRequired();

        builder.Property(e => e.MotivoEscalacion)
            .HasColumnName("motivo_escalacion")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.DiagnosticoInicial)
            .HasColumnName("diagnostico_inicial")
            .HasColumnType("TEXT");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(e => e.Ticket)
            .WithMany(t => t.Escalaciones)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.EscaladoPor)
            .WithMany()
            .HasForeignKey(e => e.EscaladoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AsignadoA)
            .WithMany()
            .HasForeignKey(e => e.AsignadoAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TicketId);
    }
}
