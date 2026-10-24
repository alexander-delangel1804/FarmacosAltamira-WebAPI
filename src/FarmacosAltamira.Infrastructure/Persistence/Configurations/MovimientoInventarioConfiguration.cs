using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> b)
    {
        b.ToTable("movimiento_inventario");
        b.HasKey(x => x.IdMovimiento);
        b.Property(x => x.IdMovimiento).UseIdentityAlwaysColumn();
        b.Property(x => x.IdLote).IsRequired();
        b.Property(x => x.IdUbicacion).IsRequired();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.FechaHora).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Tipo).HasConversion(new SnakeUpperEnumConverter<TipoMovimientoInventario>()).HasMaxLength(20).IsRequired();
        b.Property(x => x.Cantidad).IsRequired();
        b.Property(x => x.Motivo).HasMaxLength(150);
        b.Property(x => x.DocumentoOrigen).HasMaxLength(30);
        b.Property(x => x.IdDocumento);

        b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.IdLote);
        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);

        b.HasIndex(x => new { x.IdLote, x.IdUbicacion, x.FechaHora });
        b.HasIndex(x => new { x.DocumentoOrigen, x.IdDocumento });
    }
}