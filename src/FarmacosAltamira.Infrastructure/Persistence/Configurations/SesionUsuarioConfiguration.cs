using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class SesionUsuarioConfiguration : IEntityTypeConfiguration<SesionUsuario>
{
    public void Configure(EntityTypeBuilder<SesionUsuario> b)
    {
        b.ToTable("sesion_usuario");
        b.HasKey(x => x.IdSesion);
        b.Property(x => x.IdSesion).UseIdentityAlwaysColumn();
        b.Property(x => x.IdUsuario).IsRequired();
        b.Property(x => x.HashToken).HasMaxLength(255).IsRequired();
        b.Property(x => x.Expiracion).IsRequired();
        b.Property(x => x.Revocado).HasDefaultValue(false);

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
        b.HasIndex(x => new { x.IdUsuario, x.Revocado });
        b.HasIndex(x => x.Expiracion);
    }
}