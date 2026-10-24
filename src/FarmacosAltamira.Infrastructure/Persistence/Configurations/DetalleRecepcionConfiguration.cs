using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class DetalleRecepcionConfiguration : IEntityTypeConfiguration<DetalleRecepcion>
{
    public void Configure(EntityTypeBuilder<DetalleRecepcion> b)
    {
        b.ToTable("detalle_recepcion");
        b.HasKey(x => new { x.IdRecepcion, x.IdLote });
        b.Property(x => x.CantidadRecibida).IsRequired();

        b.HasOne(x => x.Recepcion).WithMany(r => r.Detalles).HasForeignKey(x => x.IdRecepcion);
        b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.IdLote);
    }
}