using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Commercial;

namespace OpsHub.Infrastructure.Persistence.Configurations.Commercial;

public class ContactoClienteConfiguration : IEntityTypeConfiguration<ContactoCliente>
{
    public void Configure(EntityTypeBuilder<ContactoCliente> builder)
    {
        builder.ToTable("contactos_cliente");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.ClienteId).HasColumnName("cliente_id").IsRequired();

        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Cargo)
            .HasColumnName("cargo")
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(25);

        builder.Property(c => c.EsPrincipal)
            .HasColumnName("es_principal")
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(c => c.Cliente)
            .WithMany(cl => cl.Contactos)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ClienteId);
    }
}
