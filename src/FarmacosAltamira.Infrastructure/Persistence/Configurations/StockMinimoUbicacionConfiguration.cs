using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class StockMinimoUbicacionConfiguration : IEntityTypeConfiguration<StockMinimoUbicacion>
{
    public void Configure(EntityTypeBuilder<StockMinimoUbicacion> b)
    {
        b.ToTable("stock_minimo_ubicacion");
        b.HasKey(x => new { x.IdProducto, x.IdUbicacion });
        b.Property(x => x.StockMinimo).IsRequired();
        b.HasOne(x => x.Producto).WithMany(p => p.StockMinimoUbicaciones).HasForeignKey(x => x.IdProducto);
        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
    }
}