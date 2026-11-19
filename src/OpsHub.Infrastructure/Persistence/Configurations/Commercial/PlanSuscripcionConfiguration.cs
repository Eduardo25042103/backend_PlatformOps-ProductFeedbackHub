using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Commercial;

namespace OpsHub.Infrastructure.Persistence.Configurations.Commercial;

public class PlanSuscripcionConfiguration : IEntityTypeConfiguration<PlanSuscripcion>
{
    public void Configure(EntityTypeBuilder<PlanSuscripcion> builder)
    {
        builder.ToTable("planes_suscripcion");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(p => p.Codigo).IsUnique();

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.PrecioMensual)
            .HasColumnName("precio_mensual")
            .HasPrecision(10, 2)
            .HasDefaultValue(0.00m);

        builder.Property(p => p.LimiteUsuarios)
            .HasColumnName("limite_usuarios")
            .HasDefaultValue(5);

        builder.Property(p => p.SlaHorasResolucion)
            .HasColumnName("sla_horas_resolucion")
            .HasDefaultValue(24);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
