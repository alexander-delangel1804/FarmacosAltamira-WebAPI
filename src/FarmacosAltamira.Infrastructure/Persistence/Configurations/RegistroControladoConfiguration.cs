using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class RegistroControladoConfiguration : IEntityTypeConfiguration<RegistroControlado>
{
    public void Configure(EntityTypeBuilder<RegistroControlado> b)
    {
        b.ToTable("registro_controlado");
        b.HasKey(x => x.IdRegistro);
        b.Property(x => x.IdRegistro).UseIdentityAlwaysColumn();
        b.Property(x => x.IdReceta).IsRequired();
        b.Property(x => x.IdDetalle).IsRequired();
        b.HasIndex(x => x.IdDetalle).IsUnique();
        b.Property(x => x.IdProducto).IsRequired();
        b.Property(x => x.IdLote).IsRequired();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.FechaHora).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.CantidadSurtida).IsRequired();
        b.Property(x => x.NombrePaciente).HasMaxLength(100).IsRequired();
        b.Property(x => x.ExistenciaPosterior).IsRequired();

        b.HasOne(x => x.Receta).WithMany(r => r.RegistrosControlados).HasForeignKey(x => x.IdReceta);
        b.HasOne(x => x.DetalleVenta).WithOne(d => d.RegistroControlado).HasForeignKey<RegistroControlado>(x => x.IdDetalle);
        b.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto);
        b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.IdLote);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
    }
}