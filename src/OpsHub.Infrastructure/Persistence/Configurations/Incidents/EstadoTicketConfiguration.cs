using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Infrastructure.Persistence.Configurations.Incidents;

public class EstadoTicketConfiguration : IEntityTypeConfiguration<EstadoTicket>
{
    public void Configure(EntityTypeBuilder<EstadoTicket> builder)
    {
        builder.ToTable("estados_ticket");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(e => e.Codigo).IsUnique();

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(7)
            .HasDefaultValue("#64748B");

        builder.Property(e => e.EsFinal)
            .HasColumnName("es_final")
            .HasDefaultValue(false);

        builder.Property(e => e.Orden)
            .HasColumnName("orden")
            .HasDefaultValue(1);
    }
}
