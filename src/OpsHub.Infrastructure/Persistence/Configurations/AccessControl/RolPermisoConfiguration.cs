using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Infrastructure.Persistence.Configurations.AccessControl;

public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("roles_permisos");

        builder.HasKey(rp => new { rp.RolId, rp.PermisoId });

        builder.Property(rp => rp.RolId).HasColumnName("rol_id");
        builder.Property(rp => rp.PermisoId).HasColumnName("permiso_id");

        builder.HasOne(rp => rp.Rol)
            .WithMany(r => r.RolesPermisos)
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permiso)
            .WithMany(p => p.RolesPermisos)
            .HasForeignKey(rp => rp.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
