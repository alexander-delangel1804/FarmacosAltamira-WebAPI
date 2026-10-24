using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> b)
    {
        b.ToTable("detalle_venta");
        b.HasKey(x => x.IdDetalle);
        b.Property(x => x.IdDetalle).UseIdentityAlwaysColumn();
        b.Property(x => x.IdVenta).IsRequired();
        b.Property(x => x.IdProducto).IsRequired();
        b.Property(x => x.IdLote).IsRequired();
        b.Property(x => x.IdPromocion);
        b.Property(x => x.Cantidad).IsRequired();
        b.Property(x => x.PrecioUnitario).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.Descuento).HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
        b.Property(x => x.Importe).HasPrecision(12, 2).IsRequired();

        b.HasOne(x => x.Venta).WithMany(v => v.Detalles).HasForeignKey(x => x.IdVenta);
        b.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto);
        b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.IdLote);
        b.HasOne(x => x.Promocion).WithMany().HasForeignKey(x => x.IdPromocion);
        b.HasIndex(x => x.IdVenta);
        b.HasIndex(x => x.IdLote);
    }
}