using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Commercial;

namespace OpsHub.Infrastructure.Persistence.Configurations.Commercial;

public class NotaClienteConfiguration : IEntityTypeConfiguration<NotaCliente>
{
    public void Configure(EntityTypeBuilder<NotaCliente> builder)
    {
        builder.ToTable("notas_cliente");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");

        builder.Property(n => n.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(n => n.UsuarioId).HasColumnName("usuario_id").IsRequired();

        builder.Property(n => n.TipoNota)
            .HasColumnName("tipo_nota")
            .HasMaxLength(30)
            .HasDefaultValue("GENERAL");

        builder.Property(n => n.Nota)
            .HasColumnName("nota")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(n => n.Cliente)
            .WithMany(cl => cl.Notas)
            .HasForeignKey(n => n.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Usuario)
            .WithMany()
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
