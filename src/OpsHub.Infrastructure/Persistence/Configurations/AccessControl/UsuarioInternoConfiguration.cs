using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Infrastructure.Persistence.Configurations.AccessControl;

public class UsuarioInternoConfiguration : IEntityTypeConfiguration<UsuarioInterno>
{
    public void Configure(EntityTypeBuilder<UsuarioInterno> builder)
    {
        builder.ToTable("usuarios_internos");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.CognitoSub)
            .HasColumnName("cognito_sub")
            .HasMaxLength(64);
        builder.HasIndex(u => u.CognitoSub).IsUnique();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255);

        builder.Property(u => u.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Apellido)
            .HasColumnName("apellido")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(25);

        builder.Property(u => u.DepartamentoId)
            .HasColumnName("departamento_id")
            .IsRequired();

        builder.Property(u => u.RolId)
            .HasColumnName("rol_id")
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.UltimoLogin)
            .HasColumnName("ultimo_login");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(u => u.Departamento)
            .WithMany(d => d.Usuarios)
            .HasForeignKey(u => u.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
