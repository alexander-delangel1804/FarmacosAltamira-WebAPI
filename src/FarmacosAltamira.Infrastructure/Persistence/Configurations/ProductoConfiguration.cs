using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> b)
    {
        b.ToTable("producto");
        b.HasKey(x => x.IdProducto);
        b.Property(x => x.IdProducto).UseIdentityAlwaysColumn();
        b.Property(x => x.IdCategoria).IsRequired();
        b.Property(x => x.CodigoBarras).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.CodigoBarras).IsUnique();
        b.Property(x => x.NombreComercial).HasMaxLength(120).IsRequired();
        b.Property(x => x.SustanciaActiva).HasMaxLength(120);
        b.Property(x => x.Presentacion).HasMaxLength(80);
        b.Property(x => x.PrecioVenta).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.TasaIva).HasPrecision(4, 2).HasDefaultValue(0).IsRequired();
        b.Property(x => x.StockMinimo).HasDefaultValue(0).IsRequired();
        b.Property(x => x.EsControlado).HasDefaultValue(false).IsRequired();
        b.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        b.HasOne(x => x.Categoria).WithMany(c => c.Productos).HasForeignKey(x => x.IdCategoria);
        b.HasOne(x => x.DatosFiscales).WithOne(pf => pf.Producto).HasForeignKey<ProductoFiscal>(pf => pf.IdProducto);
        b.HasIndex(x => x.Activo).HasFilter("activo = true");
    }
}