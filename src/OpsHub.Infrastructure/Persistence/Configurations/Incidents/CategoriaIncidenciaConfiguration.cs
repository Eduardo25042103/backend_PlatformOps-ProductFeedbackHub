using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Infrastructure.Persistence.Configurations.Incidents;

public class CategoriaIncidenciaConfiguration : IEntityTypeConfiguration<CategoriaIncidencia>
{
    public void Configure(EntityTypeBuilder<CategoriaIncidencia> builder)
    {
        builder.ToTable("categorias_incidencia");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(c => c.Codigo).IsUnique();

        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(255);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
    }
}
