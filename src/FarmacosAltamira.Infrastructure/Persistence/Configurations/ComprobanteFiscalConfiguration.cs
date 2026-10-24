using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ComprobanteFiscalConfiguration : IEntityTypeConfiguration<ComprobanteFiscal>
{
    public void Configure(EntityTypeBuilder<ComprobanteFiscal> b)
    {
        b.ToTable("comprobante_fiscal");
        b.HasKey(x => x.IdComprobante);
        b.Property(x => x.IdComprobante).UseIdentityAlwaysColumn();
        b.Property(x => x.IdVenta).IsRequired();
        b.HasIndex(x => x.IdVenta).IsUnique();
        b.Property(x => x.IdCliente);
        b.Property(x => x.IdEmisor);
        b.Property(x => x.UuidCfdi);
        b.HasIndex(x => x.UuidCfdi).IsUnique().HasFilter("uuid_cfdi IS NOT NULL");
        b.Property(x => x.Serie).HasMaxLength(10);
        b.Property(x => x.Folio).HasMaxLength(20);
        b.Property(x => x.FechaTimbrado);
        b.Property(x => x.Estatus).HasConversion(new SnakeUpperEnumConverter<EstatusComprobanteFiscal>()).HasMaxLength(15).HasDefaultValue(EstatusComprobanteFiscal.Pendiente).IsRequired();
        b.Property(x => x.Xml);
        b.Property(x => x.Reintentos).HasDefaultValue(0).IsRequired();
        b.Property(x => x.UltimoError).HasMaxLength(200);
        b.Property(x => x.ProximoReintento);
        b.Property(x => x.XmlHash).HasMaxLength(64);

        b.HasOne(x => x.Venta).WithOne(v => v.Comprobante).HasForeignKey<ComprobanteFiscal>(x => x.IdVenta);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente);
        b.HasOne(x => x.Emisor).WithMany(e => e.Comprobantes).HasForeignKey(x => x.IdEmisor);
        b.HasIndex(x => x.Estatus).HasFilter("estatus IN ('PENDIENTE','ERROR')");
    }
}