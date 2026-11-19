using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.Incidents;

namespace OpsHub.Infrastructure.Persistence.Configurations.Incidents;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(25)
            .IsRequired();
        builder.HasIndex(t => t.Codigo).IsUnique();

        builder.Property(t => t.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(t => t.ReportadoPorId).HasColumnName("reportado_por_id").IsRequired();
        builder.Property(t => t.AsignadoAId).HasColumnName("asignado_a_id");
        builder.Property(t => t.CategoriaId).HasColumnName("categoria_id").IsRequired();
        builder.Property(t => t.PrioridadId).HasColumnName("prioridad_id").IsRequired();
        builder.Property(t => t.EstadoId).HasColumnName("estado_id").IsRequired();

        builder.Property(t => t.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(t => t.TelefonoContacto)
            .HasColumnName("telefono_contacto")
            .HasMaxLength(25);

        builder.Property(t => t.CanalOrigen)
            .HasColumnName("canal_origen")
            .HasMaxLength(20)
            .HasDefaultValue("WEB");

        // Bloqueo Optimista: IsConcurrencyToken intercepta modificaciones concurrentes (HTTP 409)
        builder.Property(t => t.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(t => t.FechaCierre)
            .HasColumnName("fecha_cierre");

        // Relaciones
        builder.HasOne(t => t.Cliente)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ReportadoPor)
            .WithMany()
            .HasForeignKey(t => t.ReportadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AsignadoA)
            .WithMany()
            .HasForeignKey(t => t.AsignadoAId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Categoria)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Prioridad)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PrioridadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Estado)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(t => t.EstadoId);
        builder.HasIndex(t => t.PrioridadId);
        builder.HasIndex(t => t.AsignadoAId);
        builder.HasIndex(t => t.CreatedAt);
    }
}
