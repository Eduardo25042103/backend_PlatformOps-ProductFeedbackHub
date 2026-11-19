using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Commercial;

namespace OpsHub.Infrastructure.Persistence.Configurations.Commercial;

public class ClienteSaasConfiguration : IEntityTypeConfiguration<ClienteSaas>
{
    public void Configure(EntityTypeBuilder<ClienteSaas> builder)
    {
        builder.ToTable("clientes_saas");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Ruc)
            .HasColumnName("ruc")
            .HasMaxLength(11)
            .IsRequired();
        builder.HasIndex(c => c.Ruc).IsUnique();

        builder.Property(c => c.RazonSocial)
            .HasColumnName("razon_social")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.NombreComercial)
            .HasColumnName("nombre_comercial")
            .HasMaxLength(150);

        builder.Property(c => c.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(c => c.MontoAnualContratado)
            .HasColumnName("monto_anual_contratado")
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m);

        builder.Property(c => c.EstadoCuenta)
            .HasColumnName("estado_cuenta")
            .HasMaxLength(20)
            .HasDefaultValue("ACTIVO");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(c => c.Plan)
            .WithMany(p => p.Clientes)
            .HasForeignKey(c => c.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
