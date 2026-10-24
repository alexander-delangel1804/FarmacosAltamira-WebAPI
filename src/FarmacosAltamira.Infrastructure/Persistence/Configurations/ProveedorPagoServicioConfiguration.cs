using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ProveedorPagoServicioConfiguration : IEntityTypeConfiguration<ProveedorPagoServicio>
{
    public void Configure(EntityTypeBuilder<ProveedorPagoServicio> b)
    {
        b.ToTable("proveedor_pago_servicio");
        b.HasKey(x => x.IdProveedorPago);
        b.Property(x => x.IdProveedorPago).UseIdentityAlwaysColumn();
        b.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.Activo).HasDefaultValue(true).IsRequired();
    }
}