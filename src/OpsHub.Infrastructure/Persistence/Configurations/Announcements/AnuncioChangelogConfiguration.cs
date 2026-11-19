using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Announcements;

namespace OpsHub.Infrastructure.Persistence.Configurations.Announcements;

public class AnuncioChangelogConfiguration : IEntityTypeConfiguration<AnuncioChangelog>
{
    public void Configure(EntityTypeBuilder<AnuncioChangelog> builder)
    {
        builder.ToTable("anuncios_changelog");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.VersionTag)
            .HasColumnName("version_tag")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Resumen)
            .HasColumnName("resumen")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(a => a.RedactadoPorId).HasColumnName("redactado_por_id").IsRequired();

        builder.Property(a => a.FechaLanzamiento)
            .HasColumnName("fecha_lanzamiento")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(a => a.RedactadoPor)
            .WithMany()
            .HasForeignKey(a => a.RedactadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
