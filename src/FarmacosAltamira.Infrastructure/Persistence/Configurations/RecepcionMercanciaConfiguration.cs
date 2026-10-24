using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class RecepcionMercanciaConfiguration : IEntityTypeConfiguration<RecepcionMercancia>
{
    public void Configure(EntityTypeBuilder<RecepcionMercancia> b)
    {
        b.ToTable("recepcion_mercancia");
        b.HasKey(x => x.IdRecepcion);
        b.Property(x => x.IdRecepcion).UseIdentityAlwaysColumn();
        b.Property(x => x.IdUbicacion).IsRequired();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.FechaHora).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.DocumentoReferencia).HasMaxLength(40);
        b.Property(x => x.Observaciones).HasMaxLength(200);

        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
    }
}