using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class CajaConfiguration : IEntityTypeConfiguration<Caja>
{
    public void Configure(EntityTypeBuilder<Caja> b)
    {
        b.ToTable("caja");
        b.HasKey(x => x.IdCaja);
        b.Property(x => x.IdCaja).UseIdentityAlwaysColumn();
        b.Property(x => x.IdUbicacion).IsRequired();
        b.Property(x => x.Numero).IsRequired();
        b.Property(x => x.Activa).HasDefaultValue(true).IsRequired();
        b.HasIndex(x => new { x.IdUbicacion, x.Numero }).IsUnique();

        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
    }
}