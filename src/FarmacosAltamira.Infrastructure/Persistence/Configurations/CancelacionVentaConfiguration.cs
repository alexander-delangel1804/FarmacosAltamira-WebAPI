using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class CancelacionVentaConfiguration : IEntityTypeConfiguration<CancelacionVenta>
{
    public void Configure(EntityTypeBuilder<CancelacionVenta> b)
    {
        b.ToTable("cancelacion_venta");
        b.HasKey(x => x.IdCancelacion);
        b.Property(x => x.IdCancelacion).UseIdentityAlwaysColumn();
        b.Property(x => x.IdVenta).IsRequired();
        b.HasIndex(x => x.IdVenta).IsUnique();
        b.Property(x => x.IdUsuarioSolicita).IsRequired();
        b.Property(x => x.IdUsuarioAutoriza).IsRequired();
        b.Property(x => x.FechaHora).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Motivo).HasMaxLength(150).IsRequired();
        b.Property(x => x.CfdiCancelado).HasDefaultValue(false).IsRequired();

        b.HasOne(x => x.Venta).WithMany().HasForeignKey(x => x.IdVenta);
        b.HasOne(x => x.UsuarioSolicita).WithMany().HasForeignKey(x => x.IdUsuarioSolicita);
        b.HasOne(x => x.UsuarioAutoriza).WithMany().HasForeignKey(x => x.IdUsuarioAutoriza);
    }
}