using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.OperationsAndAudit;

namespace OpsHub.Infrastructure.Persistence.Configurations.OperationsAndAudit;

public class LogActividadConfiguration : IEntityTypeConfiguration<LogActividad>
{
    public void Configure(EntityTypeBuilder<LogActividad> builder)
    {
        builder.ToTable("logs_actividad");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.UsuarioId).HasColumnName("usuario_id");
        builder.Property(l => l.Accion).HasColumnName("accion").HasMaxLength(50).IsRequired();
        builder.Property(l => l.TablaAfectada).HasColumnName("tabla_afectada").HasMaxLength(50).IsRequired();
        builder.Property(l => l.RegistroId).HasColumnName("registro_id").IsRequired();
        builder.Property(l => l.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45);
        builder.Property(l => l.Plataforma).HasColumnName("plataforma").HasMaxLength(20);

        builder.Property(l => l.ValoresAnterioresJson)
            .HasColumnName("valores_anteriores_json")
            .HasColumnType("JSON");

        builder.Property(l => l.ValoresNuevosJson)
            .HasColumnName("valores_nuevos_json")
            .HasColumnType("JSON");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(l => l.Usuario)
            .WithMany()
            .HasForeignKey(l => l.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(l => new { l.TablaAfectada, l.RegistroId })
            .HasDatabaseName("idx_logs_tabla_registro");

        builder.HasIndex(l => l.CreatedAt)
            .HasDatabaseName("idx_logs_created_at");
    }
}
