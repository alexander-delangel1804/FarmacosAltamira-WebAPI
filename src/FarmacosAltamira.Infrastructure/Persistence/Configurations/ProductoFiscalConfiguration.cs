using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ProductoFiscalConfiguration : IEntityTypeConfiguration<ProductoFiscal>
{
    public void Configure(EntityTypeBuilder<ProductoFiscal> b)
    {
        b.ToTable("producto_fiscal");
        b.HasKey(x => x.IdProducto);
        b.Property(x => x.ClaveProdServ).HasMaxLength(8).IsRequired();
        b.Property(x => x.ClaveUnidad).HasMaxLength(3).IsRequired();
        b.Property(x => x.ObjetoImp).HasMaxLength(2).HasDefaultValue("02").IsRequired();
        b.Property(x => x.IepsTasa).HasPrecision(6, 4);

        b.HasOne(x => x.Producto).WithOne(p => p.DatosFiscales).HasForeignKey<ProductoFiscal>(x => x.IdProducto);
    }
}