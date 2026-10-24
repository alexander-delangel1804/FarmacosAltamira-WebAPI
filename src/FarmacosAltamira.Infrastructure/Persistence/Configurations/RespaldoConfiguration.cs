using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class RespaldoConfiguration : IEntityTypeConfiguration<Respaldo>
{
    public void Configure(EntityTypeBuilder<Respaldo> b)
    {
        b.ToTable("respaldo");
        b.HasKey(x => x.IdRespaldo);
        b.Property(x => x.IdRespaldo).UseIdentityAlwaysColumn();
        b.Property(x => x.IdUsuario);
        b.Property(x => x.FechaHora).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Tipo).HasMaxLength(15).IsRequired();
        b.Property(x => x.Ruta).HasMaxLength(255).IsRequired();
        b.Property(x => x.TamanoMb).HasPrecision(10, 2);
        b.Property(x => x.Cifrado).HasDefaultValue(true);
        b.Property(x => x.Estado).HasMaxLength(15).IsRequired();
        b.Property(x => x.Verificado).HasDefaultValue(false);

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
    }
}