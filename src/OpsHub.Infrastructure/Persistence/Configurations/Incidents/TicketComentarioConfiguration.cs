using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Infrastructure.Persistence.Configurations.Incidents;

public class TicketComentarioConfiguration : IEntityTypeConfiguration<TicketComentario>
{
    public void Configure(EntityTypeBuilder<TicketComentario> builder)
    {
        builder.ToTable("ticket_comentarios");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.Property(c => c.UsuarioId).HasColumnName("usuario_id").IsRequired();

        builder.Property(c => c.Comentario)
            .HasColumnName("comentario")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(c => c.EsPrivadoTi)
            .HasColumnName("es_privado_ti")
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(c => c.Ticket)
            .WithMany(t => t.Comentarios)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TicketId);
    }
}
