using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Infrastructure.Persistence.Configurations.Incidents;

public class PrioridadConfiguration : IEntityTypeConfiguration<Prioridad>
{
    public void Configure(EntityTypeBuilder<Prioridad> builder)
    {
        builder.ToTable("prioridades");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(10)
            .IsRequired();
        builder.HasIndex(p => p.Codigo).IsUnique();

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.TiempoMaxRespuestaMin)
            .HasColumnName("tiempo_max_respuesta_min")
            .IsRequired();

        builder.Property(p => p.TiempoMaxResolucionHoras)
            .HasColumnName("tiempo_max_resolucion_horas")
            .IsRequired();

        builder.Property(p => p.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(7)
            .HasDefaultValue("#6366F1");

        builder.Property(p => p.Orden)
            .HasColumnName("orden")
            .HasDefaultValue(1);
    }
}
