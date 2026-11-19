using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Announcements;

namespace OpsHub.Infrastructure.Persistence.Configurations.Announcements;

public class ComunicadoSistemaConfiguration : IEntityTypeConfiguration<ComunicadoSistema>
{
    public void Configure(EntityTypeBuilder<ComunicadoSistema> builder)
    {
        builder.ToTable("comunicados_sistema");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.TipoId).HasColumnName("tipo_id").IsRequired();

        builder.Property(c => c.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Mensaje)
            .HasColumnName("mensaje")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(c => c.AfectaServicio)
            .HasColumnName("afecta_servicio")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.PublicadoPorId).HasColumnName("publicado_por_id").IsRequired();

        builder.Property(c => c.InicioEvento).HasColumnName("inicio_evento");
        builder.Property(c => c.FinEstimado).HasColumnName("fin_estimado");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        // Relaciones
        builder.HasOne(c => c.Tipo)
            .WithMany(t => t.Comunicados)
            .HasForeignKey(c => c.TipoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PublicadoPor)
            .WithMany()
            .HasForeignKey(c => c.PublicadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Destinatarios)
            .WithOne(d => d.Comunicado)
            .HasForeignKey(d => d.ComunicadoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
