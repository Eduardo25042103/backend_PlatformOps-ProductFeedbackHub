using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Announcements;

namespace OpsHub.Infrastructure.Persistence.Configurations.Announcements;

public class DestinatarioComunicadoConfiguration : IEntityTypeConfiguration<DestinatarioComunicado>
{
    public void Configure(EntityTypeBuilder<DestinatarioComunicado> builder)
    {
        builder.ToTable("destinatarios_comunicado");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.ComunicadoId).HasColumnName("comunicado_id").IsRequired();
        builder.Property(d => d.DepartamentoId).HasColumnName("departamento_id");
        builder.Property(d => d.ClienteId).HasColumnName("cliente_id");

        builder.HasOne(d => d.Comunicado)
            .WithMany(c => c.Destinatarios)
            .HasForeignKey(d => d.ComunicadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Departamento)
            .WithMany()
            .HasForeignKey(d => d.DepartamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Cliente)
            .WithMany()
            .HasForeignKey(d => d.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
