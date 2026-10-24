using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class CatalogoServicioConfiguration : IEntityTypeConfiguration<CatalogoServicio>
{
    public void Configure(EntityTypeBuilder<CatalogoServicio> b)
    {
        b.ToTable("catalogo_servicio");
        b.HasKey(x => x.IdServicio);
        b.Property(x => x.IdServicio).UseIdentityAlwaysColumn();
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Nombre).IsUnique();
        b.Property(x => x.Tipo).HasMaxLength(30);
        b.Property(x => x.TipoComision).HasConversion(new SnakeUpperEnumConverter<TipoComision>()).HasMaxLength(12).IsRequired();
        b.Property(x => x.ValorComision).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.Activo).HasDefaultValue(true).IsRequired();
        b.Property(x => x.IdProveedorPago);
        b.Property(x => x.CodigoProveedor).HasMaxLength(40);
        b.Property(x => x.EtiquetaReferencia).HasMaxLength(40);
        b.Property(x => x.PatronReferencia).HasMaxLength(100);
        b.Property(x => x.MontoMinimo).HasPrecision(12, 2);
        b.Property(x => x.MontoMaximo).HasPrecision(12, 2);

        b.HasOne(x => x.Proveedor).WithMany(p => p.Servicios).HasForeignKey(x => x.IdProveedorPago);
    }
}