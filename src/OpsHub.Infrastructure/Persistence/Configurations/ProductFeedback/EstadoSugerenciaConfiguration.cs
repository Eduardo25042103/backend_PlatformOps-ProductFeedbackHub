using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Infrastructure.Persistence.Configurations.ProductFeedback;

public class EstadoSugerenciaConfiguration : IEntityTypeConfiguration<EstadoSugerencia>
{
    public void Configure(EntityTypeBuilder<EstadoSugerencia> builder)
    {
        builder.ToTable("estados_sugerencia");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(e => e.Codigo).IsUnique();

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Orden)
            .HasColumnName("orden")
            .HasDefaultValue(1);
    }
}
