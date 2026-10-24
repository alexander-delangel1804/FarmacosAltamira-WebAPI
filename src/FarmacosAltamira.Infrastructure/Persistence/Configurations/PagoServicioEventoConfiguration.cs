using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class PagoServicioEventoConfiguration : IEntityTypeConfiguration<PagoServicioEvento>
{
    public void Configure(EntityTypeBuilder<PagoServicioEvento> b)
    {
        b.ToTable("pago_servicio_evento");
        b.HasKey(x => x.IdEvento);
        b.Property(x => x.IdEvento).UseIdentityAlwaysColumn();
        b.Property(x => x.IdPagoServicio).IsRequired();
        b.Property(x => x.Fecha).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Operacion).HasMaxLength(20).IsRequired();
        b.Property(x => x.EstadoProveedor).HasMaxLength(20);
        b.Property(x => x.CodigoProveedor).HasMaxLength(30);
        b.Property(x => x.Mensaje);

        b.HasOne(x => x.PagoServicio).WithMany(p => p.Eventos).HasForeignKey(x => x.IdPagoServicio);
    }
}