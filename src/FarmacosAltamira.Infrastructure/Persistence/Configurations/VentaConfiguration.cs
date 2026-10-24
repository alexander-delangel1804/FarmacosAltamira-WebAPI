using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> b)
    {
        b.ToTable("venta");
        b.HasKey(x => x.IdVenta);
        b.Property(x => x.IdVenta).UseIdentityAlwaysColumn();
        b.Property(x => x.IdCorte).IsRequired();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.Folio).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Folio).IsUnique();
        b.Property(x => x.UuidOperacion).IsRequired();
        b.HasIndex(x => x.UuidOperacion).IsUnique();
        b.Property(x => x.FechaHora).IsRequired();
        b.Property(x => x.Subtotal).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.Descuento).HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
        b.Property(x => x.Iva).HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
        b.Property(x => x.Total).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.Estado).HasConversion(new SnakeUpperEnumConverter<EstadoVenta>()).HasMaxLength(25).HasDefaultValue(EstadoVenta.EnCaptura).IsRequired();
        b.Property(x => x.RequiereFactura).HasDefaultValue(false).IsRequired();
        b.Property(x => x.Sincronizada).HasDefaultValue(true).IsRequired();
        b.Property(x => x.FechaSincronizacion);

        b.HasOne(x => x.Corte).WithMany(c => c.Ventas).HasForeignKey(x => x.IdCorte);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
        b.HasIndex(x => x.IdCorte);
        b.HasIndex(x => x.FechaHora);
        b.HasIndex(x => x.Estado).HasFilter("estado = 'FACTURACION_PENDIENTE'");
        b.HasIndex(x => x.UuidOperacion).IsUnique();
    }
}