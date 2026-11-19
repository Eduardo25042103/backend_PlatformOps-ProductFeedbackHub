using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Infrastructure.Persistence.Configurations.ProductFeedback;

public class VotoSugerenciaConfiguration : IEntityTypeConfiguration<VotoSugerencia>
{
    public void Configure(EntityTypeBuilder<VotoSugerencia> builder)
    {
        builder.ToTable("votos_sugerencia");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");

        builder.Property(v => v.SugerenciaId).HasColumnName("sugerencia_id").IsRequired();
        builder.Property(v => v.UsuarioId).HasColumnName("usuario_id").IsRequired();

        builder.Property(v => v.MotivoVoto)
            .HasColumnName("motivo_voto")
            .HasMaxLength(255);

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Clave única (sugerencia_id, usuario_id)
        builder.HasIndex(v => new { v.SugerenciaId, v.UsuarioId })
            .IsUnique()
            .HasDatabaseName("uq_sugerencia_usuario");

        builder.HasOne(v => v.Sugerencia)
            .WithMany(s => s.Votos)
            .HasForeignKey(v => v.SugerenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
