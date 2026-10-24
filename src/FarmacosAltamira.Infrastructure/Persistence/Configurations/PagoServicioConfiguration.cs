using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class PagoServicioConfiguration : IEntityTypeConfiguration<PagoServicio>
{
    public void Configure(EntityTypeBuilder<PagoServicio> b)
    {
        b.ToTable("pago_servicio");
        b.HasKey(x => x.IdPagoServicio);
        b.Property(x => x.IdPagoServicio).UseIdentityAlwaysColumn();
        b.Property(x => x.IdServicio).IsRequired();
        b.Property(x => x.IdUbicacion).IsRequired();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.IdCorte).IsRequired();
        b.Property(x => x.Folio).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Folio).IsUnique();
        b.Property(x => x.FechaHora).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.ReferenciaCliente).HasMaxLength(60).IsRequired();
        b.Property(x => x.Monto).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.Comision).HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
        b.Property(x => x.TotalCobrado).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.FolioAutorizacion).HasMaxLength(30);
        b.Property(x => x.Estatus).HasConversion(new SnakeUpperEnumConverter<EstatusPagoServicio>()).HasMaxLength(15).HasDefaultValue(EstatusPagoServicio.Pendiente).IsRequired();
        b.Property(x => x.UuidOperacion);
        b.HasIndex(x => x.UuidOperacion).IsUnique().HasFilter("uuid_operacion IS NOT NULL");
        b.Property(x => x.Intentos).HasDefaultValue(0).IsRequired();
        b.Property(x => x.FechaRespuesta);
        b.Property(x => x.CodigoRespuesta).HasMaxLength(30);

        b.HasOne(x => x.Servicio).WithMany(s => s.Pagos).HasForeignKey(x => x.IdServicio);
        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
        b.HasOne(x => x.Corte).WithMany(c => c.PagosServicio).HasForeignKey(x => x.IdCorte);
    }
}