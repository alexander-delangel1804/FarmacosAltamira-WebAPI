using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class CorteCajaConfiguration : IEntityTypeConfiguration<CorteCaja>
{
    public void Configure(EntityTypeBuilder<CorteCaja> b)
    {
        b.ToTable("corte_caja");
        b.HasKey(x => x.IdCorte);
        b.Property(x => x.IdCorte).UseIdentityAlwaysColumn();
        b.Property(x => x.IdCaja).IsRequired();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.FechaApertura).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.FechaCierre);
        b.Property(x => x.FondoInicial).HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
        b.Property(x => x.TotalSistema).HasPrecision(12, 2);
        b.Property(x => x.TotalContado).HasPrecision(12, 2);
        b.Property(x => x.Diferencia).HasPrecision(12, 2);
        b.Property(x => x.Estado).HasConversion(new SnakeUpperEnumConverter<EstadoCorte>()).HasMaxLength(10).HasDefaultValue(EstadoCorte.Abierto).IsRequired();
        b.Property(x => x.UuidOperacion);

        b.HasOne(x => x.Caja).WithMany(c => c.Cortes).HasForeignKey(x => x.IdCaja);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
        b.HasIndex(x => x.IdCaja).HasFilter("estado = 'ABIERTO'").IsUnique();
        b.HasIndex(x => x.UuidOperacion).IsUnique().HasFilter("uuid_operacion IS NOT NULL");
    }
}