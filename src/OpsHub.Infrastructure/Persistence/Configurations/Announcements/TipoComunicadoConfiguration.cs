using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Announcements;

namespace OpsHub.Infrastructure.Persistence.Configurations.Announcements;

public class TipoComunicadoConfiguration : IEntityTypeConfiguration<TipoComunicado>
{
    public void Configure(EntityTypeBuilder<TipoComunicado> builder)
    {
        builder.ToTable("tipos_comunicado");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(t => t.Codigo).IsUnique();

        builder.Property(t => t.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(7)
            .HasDefaultValue("#3B82F6")
            .IsRequired();
    }
}
