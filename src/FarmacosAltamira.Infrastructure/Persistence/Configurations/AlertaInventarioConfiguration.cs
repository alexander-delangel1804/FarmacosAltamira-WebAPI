using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class AlertaInventarioConfiguration : IEntityTypeConfiguration<AlertaInventario>
{
    public void Configure(EntityTypeBuilder<AlertaInventario> b)
    {
        b.ToTable("alerta_inventario");
        b.HasKey(x => x.IdAlerta);
        b.Property(x => x.IdAlerta).UseIdentityAlwaysColumn();
        b.Property(x => x.IdProducto).IsRequired();
        b.Property(x => x.IdUbicacion).IsRequired();
        b.Property(x => x.IdLote);
        b.Property(x => x.Tipo).HasConversion(new SnakeUpperEnumConverter<TipoAlertaInventario>()).HasMaxLength(20).IsRequired();
        b.Property(x => x.FechaGeneracion).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Mensaje).HasMaxLength(200);
        b.Property(x => x.Atendida).HasDefaultValue(false).IsRequired();

        b.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto);
        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
        b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.IdLote);
        b.HasIndex(x => new { x.IdUbicacion, x.Tipo }).HasFilter("atendida = false");
    }
}