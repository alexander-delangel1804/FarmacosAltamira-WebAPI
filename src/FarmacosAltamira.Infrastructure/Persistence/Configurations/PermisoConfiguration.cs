using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> b)
    {
        b.ToTable("permiso");
        b.HasKey(x => x.IdPermiso);
        b.Property(x => x.IdPermiso).UseIdentityAlwaysColumn();
        b.Property(x => x.Modulo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Accion).HasMaxLength(40).IsRequired();
        b.HasIndex(x => new { x.Modulo, x.Accion }).IsUnique();
    }
}