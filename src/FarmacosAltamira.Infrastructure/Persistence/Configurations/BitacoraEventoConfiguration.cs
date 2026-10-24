using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class BitacoraEventoConfiguration : IEntityTypeConfiguration<BitacoraEvento>
{
    public void Configure(EntityTypeBuilder<BitacoraEvento> b)
    {
        b.ToTable("bitacora_evento");
        b.HasKey(x => x.IdEvento);
        b.Property(x => x.IdEvento).UseIdentityAlwaysColumn();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.IdUbicacion);
        b.Property(x => x.FechaHora).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Modulo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Accion).HasMaxLength(60).IsRequired();
        b.Property(x => x.Entidad).HasMaxLength(40);
        b.Property(x => x.IdEntidad).HasMaxLength(40);
        b.Property(x => x.Detalle);
        b.Property(x => x.IpOrigen).HasMaxLength(45);

        b.HasIndex(x => x.FechaHora);
        b.HasIndex(x => new { x.IdUsuario, x.FechaHora });
        b.HasIndex(x => new { x.Modulo, x.FechaHora });

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
    }
}