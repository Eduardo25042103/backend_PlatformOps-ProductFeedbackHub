using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.ProductFeedback;

namespace OpsHub.Infrastructure.Persistence.Configurations.ProductFeedback;

public class ClienteSolicitanteConfiguration : IEntityTypeConfiguration<ClienteSolicitante>
{
    public void Configure(EntityTypeBuilder<ClienteSolicitante> builder)
    {
        builder.ToTable("clientes_solicitantes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.SugerenciaId).HasColumnName("sugerencia_id").IsRequired();
        builder.Property(c => c.ClienteId).HasColumnName("cliente_id").IsRequired();

        builder.Property(c => c.ValorAnualRepresentado)
            .HasColumnName("valor_anual_representado")
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m);

        builder.Property(c => c.OrigenWebhook)
            .HasColumnName("origen_webhook")
            .HasMaxLength(50)
            .HasDefaultValue("MANUAL");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Clave única (sugerencia_id, cliente_id)
        builder.HasIndex(c => new { c.SugerenciaId, c.ClienteId })
            .IsUnique()
            .HasDatabaseName("uq_sugerencia_cliente");

        builder.HasOne(c => c.Sugerencia)
            .WithMany(s => s.ClientesSolicitantes)
            .HasForeignKey(c => c.SugerenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Cliente)
            .WithMany(cl => cl.SugerenciasSolicitadas)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
