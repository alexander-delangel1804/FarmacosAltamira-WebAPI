using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class EmisorFiscalConfiguration : IEntityTypeConfiguration<EmisorFiscal>
{
    public void Configure(EntityTypeBuilder<EmisorFiscal> b)
    {
        b.ToTable("emisor_fiscal");
        b.HasKey(x => x.IdEmisor);
        b.Property(x => x.IdEmisor).UseIdentityAlwaysColumn();
        b.Property(x => x.Rfc).HasMaxLength(13).IsRequired();
        b.HasIndex(x => x.Rfc).IsUnique();
        b.Property(x => x.RazonSocial).HasMaxLength(254).IsRequired();
        b.Property(x => x.RegimenFiscal).HasMaxLength(3).IsRequired();
        b.Property(x => x.CpExpedicion).HasMaxLength(5).IsRequired();
        b.Property(x => x.RefCertificado).HasMaxLength(100);
        b.Property(x => x.Activo).HasDefaultValue(true).IsRequired();
    }
}