using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class EstacionConfiguration : IEntityTypeConfiguration<Estacion>
{
    public void Configure(EntityTypeBuilder<Estacion> b)
    {
        b.ToTable("estacion");
        b.HasKey(x => x.IdEstacion);
        b.Property(x => x.IdEstacion).UseIdentityAlwaysColumn();
        b.Property(x => x.IdUbicacion).IsRequired();
        b.Property(x => x.IdCaja);
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.Property(x => x.HashSecreto).HasMaxLength(255).IsRequired();
        b.Property(x => x.Activa).HasDefaultValue(true);
        b.Property(x => x.VersionCliente).HasMaxLength(20);
        b.Property(x => x.UltimaConexion);

        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
        b.HasOne(x => x.Caja).WithMany().HasForeignKey(x => x.IdCaja);
        b.HasIndex(x => x.IdUbicacion);
    }
}