using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.OperationsAndAudit;

namespace OpsHub.Infrastructure.Persistence.Configurations.OperationsAndAudit;

public class NotificacionUsuarioConfiguration : IEntityTypeConfiguration<NotificacionUsuario>
{
    public void Configure(EntityTypeBuilder<NotificacionUsuario> builder)
    {
        builder.ToTable("notificaciones_usuario");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");

        builder.Property(n => n.UsuarioId).HasColumnName("usuario_id").IsRequired();

        builder.Property(n => n.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(n => n.Mensaje)
            .HasColumnName("mensaje")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.TipoEvento)
            .HasColumnName("tipo_evento")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Leido)
            .HasColumnName("leido")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.UrlDestino)
            .HasColumnName("url_destino")
            .HasMaxLength(255);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(n => n.Usuario)
            .WithMany()
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UsuarioId, n.Leido })
            .HasDatabaseName("idx_notificaciones_usuario_leido");
    }
}
