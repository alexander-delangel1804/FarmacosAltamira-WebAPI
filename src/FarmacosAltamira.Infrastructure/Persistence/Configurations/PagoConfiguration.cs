using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> b)
    {
        b.ToTable("pago");
        b.HasKey(x => x.IdPago);
        b.Property(x => x.IdPago).UseIdentityAlwaysColumn();
        b.Property(x => x.IdVenta).IsRequired();
        b.Property(x => x.FormaPago).HasConversion(new SnakeUpperEnumConverter<FormaPago>()).HasMaxLength(20).IsRequired();
        b.Property(x => x.Monto).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.Referencia).HasMaxLength(40);
        b.Property(x => x.Cambio).HasPrecision(12, 2).HasDefaultValue(0).IsRequired();

        b.HasOne(x => x.Venta).WithMany(v => v.Pagos).HasForeignKey(x => x.IdVenta);
        b.HasIndex(x => x.IdVenta);
    }
}