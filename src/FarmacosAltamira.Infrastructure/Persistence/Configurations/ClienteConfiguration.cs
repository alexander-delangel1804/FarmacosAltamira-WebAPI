using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("cliente");
        b.HasKey(x => x.IdCliente);
        b.Property(x => x.IdCliente).UseIdentityAlwaysColumn();
        b.Property(x => x.Rfc).HasMaxLength(13).IsRequired();
        b.Property(x => x.RazonSocial).HasMaxLength(120).IsRequired();
        b.Property(x => x.RegimenFiscal).HasMaxLength(5).IsRequired();
        b.Property(x => x.CodigoPostal).HasMaxLength(5).IsRequired();
        b.Property(x => x.UsoCfdi).HasMaxLength(4).IsRequired();
        b.Property(x => x.Correo).HasMaxLength(100);
    }
}