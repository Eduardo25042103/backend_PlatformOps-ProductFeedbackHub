using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Infrastructure.Persistence.Configurations.Incidents;

public class TicketAdjuntoConfiguration : IEntityTypeConfiguration<TicketAdjunto>
{
    public void Configure(EntityTypeBuilder<TicketAdjunto> builder)
    {
        builder.ToTable("ticket_adjuntos");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.Property(a => a.UsuarioId).HasColumnName("usuario_id").IsRequired();

        builder.Property(a => a.NombreArchivo)
            .HasColumnName("nombre_archivo")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.S3Key)
            .HasColumnName("s3_key")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.S3Url)
            .HasColumnName("s3_url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.TipoMime)
            .HasColumnName("tipo_mime")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.TamanioBytes)
            .HasColumnName("tamanio_bytes")
            .HasDefaultValue(0L);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.Adjuntos)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TicketId);
    }
}
