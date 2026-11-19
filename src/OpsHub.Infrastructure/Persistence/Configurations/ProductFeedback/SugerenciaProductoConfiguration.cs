using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Infrastructure.Persistence.Configurations.ProductFeedback;

public class SugerenciaProductoConfiguration : IEntityTypeConfiguration<SugerenciaProducto>
{
    public void Configure(EntityTypeBuilder<SugerenciaProducto> builder)
    {
        builder.ToTable("sugerencias_producto");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(25)
            .IsRequired();
        builder.HasIndex(s => s.Codigo).IsUnique();

        builder.Property(s => s.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(s => s.ModuloAfectado)
            .HasColumnName("modulo_afectado")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.EstadoId).HasColumnName("estado_id").IsRequired();
        builder.Property(s => s.CreadoPorId).HasColumnName("creado_por_id").IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(s => s.Estado)
            .WithMany(e => e.Sugerencias)
            .HasForeignKey(s => s.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CreadoPor)
            .WithMany()
            .HasForeignKey(s => s.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.EstadoId);
    }
}
