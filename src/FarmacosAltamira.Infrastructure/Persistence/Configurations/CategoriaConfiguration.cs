using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> b)
    {
        b.ToTable("categoria");
        b.HasKey(x => x.IdCategoria);
        b.Property(x => x.IdCategoria).UseIdentityAlwaysColumn();
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Nombre).IsUnique();
    }
}