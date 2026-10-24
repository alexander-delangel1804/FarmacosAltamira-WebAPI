using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuario");
        b.HasKey(x => x.IdUsuario);
        b.Property(x => x.IdUsuario).UseIdentityAlwaysColumn();
        b.Property(x => x.IdRol).IsRequired();
        b.Property(x => x.IdUbicacion);
        b.Property(x => x.NombreUsuario).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.NombreUsuario).IsUnique();
        b.Property(x => x.NombreCompleto).HasMaxLength(100).IsRequired();
        b.Property(x => x.HashContrasena).HasMaxLength(255).IsRequired();
        b.Property(x => x.Activo).HasDefaultValue(true);
        b.Property(x => x.Bloqueado).HasDefaultValue(false);
        b.Property(x => x.IntentosFallidos).HasDefaultValue(0);
        b.Property(x => x.UltimoAcceso);
        b.Property(x => x.FechaAlta).HasDefaultValueSql("current_date");
        b.Property<uint>("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        b.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol);
        b.HasOne(x => x.Ubicacion).WithMany().HasForeignKey(x => x.IdUbicacion);
    }
}