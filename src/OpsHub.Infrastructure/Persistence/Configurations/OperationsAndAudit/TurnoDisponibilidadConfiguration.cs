using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.OperationsAndAudit;

namespace OpsHub.Infrastructure.Persistence.Configurations.OperationsAndAudit;

public class TurnoDisponibilidadConfiguration : IEntityTypeConfiguration<TurnoDisponibilidad>
{
    public void Configure(EntityTypeBuilder<TurnoDisponibilidad> builder)
    {
        builder.ToTable("turnos_disponibilidad");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(t => t.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(t => t.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(t => t.EstaActivo).HasColumnName("esta_activo").HasDefaultValue(true).IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(t => t.Usuario)
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
