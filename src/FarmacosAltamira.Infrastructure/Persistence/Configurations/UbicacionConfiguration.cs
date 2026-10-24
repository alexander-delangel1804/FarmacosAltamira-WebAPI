using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class UbicacionConfiguration : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> b)
    {
        b.ToTable("ubicacion");
        b.HasKey(x => x.IdUbicacion);
        b.Property(x => x.IdUbicacion).UseIdentityAlwaysColumn();
        b.Property(x => x.Tipo).HasConversion(new SnakeUpperEnumConverter<TipoUbicacion>()).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.Property(x => x.Clave).HasMaxLength(10).IsRequired();
        b.HasIndex(x => x.Clave).IsUnique();
        b.Property(x => x.Direccion).HasMaxLength(150);
        b.Property(x => x.Municipio).HasMaxLength(60);
        b.Property(x => x.Telefono).HasMaxLength(20);
        b.Property(x => x.CapacidadReferencia).HasMaxLength(50);
        b.Property(x => x.Activa).HasDefaultValue(true);
        b.Property(x => x.IdEmisor);
        b.Property(x => x.SerieCfdi).HasMaxLength(10);

        b.HasOne(x => x.EmisorFiscal).WithMany(e => e.Ubicaciones).HasForeignKey(x => x.IdEmisor);
    }
}