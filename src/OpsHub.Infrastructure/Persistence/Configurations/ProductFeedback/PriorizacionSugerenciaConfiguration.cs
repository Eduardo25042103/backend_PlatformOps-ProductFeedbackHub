using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Infrastructure.Persistence.Configurations.ProductFeedback;

public class PriorizacionSugerenciaConfiguration : IEntityTypeConfiguration<PriorizacionSugerencia>
{
    public void Configure(EntityTypeBuilder<PriorizacionSugerencia> builder)
    {
        builder.ToTable("priorizacion_sugerencia");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.SugerenciaId).HasColumnName("sugerencia_id").IsRequired();
        builder.HasIndex(p => p.SugerenciaId).IsUnique();

        builder.Property(p => p.EsfuerzoTiDias)
            .HasColumnName("esfuerzo_ti_dias")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(p => p.ImpactoComercialScore)
            .HasColumnName("impacto_comercial_score")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(p => p.PuntuacionFinal)
            .HasColumnName("puntuacion_final")
            .HasPrecision(6, 2)
            .HasDefaultValue(0.00m)
            .IsRequired();

        builder.Property(p => p.EvaluadoPorId)
            .HasColumnName("evaluado_por_id")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(p => p.Sugerencia)
            .WithOne(s => s.Priorizacion)
            .HasForeignKey<PriorizacionSugerencia>(p => p.SugerenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.EvaluadoPor)
            .WithMany()
            .HasForeignKey(p => p.EvaluadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
