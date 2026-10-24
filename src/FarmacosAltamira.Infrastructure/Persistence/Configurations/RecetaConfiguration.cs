using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class RecetaConfiguration : IEntityTypeConfiguration<Receta>
{
    public void Configure(EntityTypeBuilder<Receta> b)
    {
        b.ToTable("receta");
        b.HasKey(x => x.IdReceta);
        b.Property(x => x.IdReceta).UseIdentityAlwaysColumn();
        b.Property(x => x.FolioReceta).HasMaxLength(20).IsRequired();
        b.Property(x => x.TipoReceta).HasConversion(new SnakeUpperEnumConverter<TipoReceta>()).HasMaxLength(20).IsRequired();
        b.Property(x => x.FechaExpedicion).HasColumnType("date").IsRequired();
        b.Property(x => x.NombreMedico).HasMaxLength(100).IsRequired();
        b.Property(x => x.CedulaMedico).HasMaxLength(20).IsRequired();
        b.Property(x => x.Institucion).HasMaxLength(100);
        b.Property(x => x.Retenida).HasDefaultValue(false).IsRequired();
    }
}