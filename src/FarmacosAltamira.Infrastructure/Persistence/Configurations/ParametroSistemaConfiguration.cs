using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ParametroSistemaConfiguration : IEntityTypeConfiguration<ParametroSistema>
{
    public void Configure(EntityTypeBuilder<ParametroSistema> b)
    {
        b.ToTable("parametro_sistema");
        b.HasKey(x => x.Clave);
        b.Property(x => x.Clave).HasMaxLength(60).IsRequired();
        b.Property(x => x.Valor).HasMaxLength(100).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(150);
        b.Property(x => x.FechaModificacion).HasDefaultValueSql("now()").IsRequired();
    }
}