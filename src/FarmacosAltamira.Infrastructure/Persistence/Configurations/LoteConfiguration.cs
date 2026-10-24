using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> b)
    {
        b.ToTable("lote");
        b.HasKey(x => x.IdLote);
        b.Property(x => x.IdLote).UseIdentityAlwaysColumn();
        b.Property(x => x.IdProducto).IsRequired();
        b.Property(x => x.NumeroLote).HasMaxLength(30).IsRequired();
        b.HasIndex(x => new { x.IdProducto, x.NumeroLote }).IsUnique();
        b.Property(x => x.FechaCaducidad).HasColumnType("date").IsRequired();
        b.Property(x => x.FechaIngreso).HasColumnType("date").HasDefaultValueSql("current_date").IsRequired();
        b.Property(x => x.Estado).HasConversion(new SnakeUpperEnumConverter<EstadoLote>()).HasMaxLength(20).HasDefaultValue(EstadoLote.Vigente).IsRequired();

        b.HasOne(x => x.Producto).WithMany(p => p.Lotes).HasForeignKey(x => x.IdProducto);
        b.HasIndex(x => new { x.IdProducto, x.FechaCaducidad });
        b.HasIndex(x => x.FechaCaducidad);
    }
}