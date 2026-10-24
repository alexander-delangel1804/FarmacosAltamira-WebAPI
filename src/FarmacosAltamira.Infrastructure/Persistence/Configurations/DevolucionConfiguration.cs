using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class DevolucionConfiguration : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> b)
    {
        b.ToTable("devolucion");
        b.HasKey(x => x.IdDevolucion);
        b.Property(x => x.IdDevolucion).UseIdentityAlwaysColumn();
        b.Property(x => x.IdVenta).IsRequired();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.Folio).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Folio).IsUnique();
        b.Property(x => x.Fecha).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Motivo).HasMaxLength(150).IsRequired();
        b.Property(x => x.MontoReembolsado).HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
        b.Property(x => x.FormaReembolso).HasMaxLength(20);

        b.HasOne(x => x.Venta).WithMany().HasForeignKey(x => x.IdVenta);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
    }
}