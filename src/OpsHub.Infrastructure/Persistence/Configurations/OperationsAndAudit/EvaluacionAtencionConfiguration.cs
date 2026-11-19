using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.OperationsAndAudit;

namespace OpsHub.Infrastructure.Persistence.Configurations.OperationsAndAudit;

public class EvaluacionAtencionConfiguration : IEntityTypeConfiguration<EvaluacionAtencion>
{
    public void Configure(EntityTypeBuilder<EvaluacionAtencion> builder)
    {
        builder.ToTable("evaluaciones_atencion");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.HasIndex(e => e.TicketId).IsUnique();

        builder.Property(e => e.PuntajeEstrellas).HasColumnName("puntaje_estrellas").IsRequired();
        builder.Property(e => e.Comentarios).HasColumnName("comentarios").HasMaxLength(500);

        builder.Property(e => e.EvaluadoEn)
            .HasColumnName("evaluado_en")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(e => e.Ticket)
            .WithOne(t => t.Evaluacion)
            .HasForeignKey<EvaluacionAtencion>(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
