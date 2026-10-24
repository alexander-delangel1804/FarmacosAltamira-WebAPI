using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class DetalleTrasladoConfiguration : IEntityTypeConfiguration<DetalleTraslado>
{
    public void Configure(EntityTypeBuilder<DetalleTraslado> b)
    {
        b.ToTable("detalle_traslado");
        b.HasKey(x => new { x.IdTraslado, x.IdLote });
        b.Property(x => x.CantidadEnviada).IsRequired();
        b.Property(x => x.CantidadRecibida);

        b.HasOne(x => x.Traslado).WithMany(t => t.Detalles).HasForeignKey(x => x.IdTraslado);
        b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.IdLote);
    }
}