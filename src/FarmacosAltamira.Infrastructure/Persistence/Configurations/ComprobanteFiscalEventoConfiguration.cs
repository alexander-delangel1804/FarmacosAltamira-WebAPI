using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ComprobanteFiscalEventoConfiguration : IEntityTypeConfiguration<ComprobanteFiscalEvento>
{
    public void Configure(EntityTypeBuilder<ComprobanteFiscalEvento> b)
    {
        b.ToTable("comprobante_fiscal_evento");
        b.HasKey(x => x.IdEvento);
        b.Property(x => x.IdEvento).UseIdentityAlwaysColumn();
        b.Property(x => x.IdComprobante).IsRequired();
        b.Property(x => x.Fecha).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
        b.Property(x => x.EstadoPac).HasMaxLength(20);
        b.Property(x => x.HttpStatus);
        b.Property(x => x.CodigoPac).HasMaxLength(30);
        b.Property(x => x.Mensaje);
        b.Property(x => x.PayloadHash).HasMaxLength(64);

        b.HasOne(x => x.Comprobante).WithMany(c => c.Eventos).HasForeignKey(x => x.IdComprobante);
    }
}