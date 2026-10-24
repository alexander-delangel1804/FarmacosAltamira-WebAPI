using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
{
    public void Configure(EntityTypeBuilder<Promocion> b)
    {
        b.ToTable("promocion");
        b.HasKey(x => x.IdPromocion);
        b.Property(x => x.IdPromocion).UseIdentityAlwaysColumn();
        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.Property(x => x.Tipo).HasConversion(new SnakeUpperEnumConverter<TipoPromocion>()).HasMaxLength(15).IsRequired();
        b.Property(x => x.Valor).HasPrecision(12, 2).IsRequired();
        b.Property(x => x.FechaInicio).HasColumnType("date").IsRequired();
        b.Property(x => x.FechaFin).HasColumnType("date").IsRequired();
        b.Property(x => x.Activa).HasDefaultValue(true).IsRequired();
        b.HasCheckConstraint("ck_promocion_fechas", "fecha_fin >= fecha_inicio");
    }
}