using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class DetalleDevolucionConfiguration : IEntityTypeConfiguration<DetalleDevolucion>
{
    public void Configure(EntityTypeBuilder<DetalleDevolucion> b)
    {
        b.ToTable("detalle_devolucion");
        b.HasKey(x => new { x.IdDevolucion, x.IdDetalle });
        b.Property(x => x.Cantidad).IsRequired();
        b.Property(x => x.ReingresaInventario).HasDefaultValue(true).IsRequired();

        b.HasOne(x => x.Devolucion).WithMany(d => d.Detalles).HasForeignKey(x => x.IdDevolucion);
        b.HasOne(x => x.DetalleVenta).WithMany().HasForeignKey(x => x.IdDetalle);
    }
}