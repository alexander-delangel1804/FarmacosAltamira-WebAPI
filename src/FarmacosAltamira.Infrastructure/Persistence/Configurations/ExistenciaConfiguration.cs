using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ExistenciaConfiguration : IEntityTypeConfiguration<Existencia>
{
    public void Configure(EntityTypeBuilder<Existencia> b)
    {
        b.ToTable("existencia");
        b.HasKey(x => new { x.IdLote, x.IdUbicacion });
        b.Property(x => x.Cantidad).IsRequired();
        b.Property(x => x.CantidadReservada).HasDefaultValue(0).IsRequired();

        b.HasOne(x => x.Lote).WithMany(l => l.Existencias).HasForeignKey(x => x.IdLote);
        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
        b.HasIndex(x => x.IdUbicacion);
    }
}