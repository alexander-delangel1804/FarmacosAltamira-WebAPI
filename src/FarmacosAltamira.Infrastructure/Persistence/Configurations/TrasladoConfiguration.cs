using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacosAltamira.Infrastructure.Persistence.Configurations;

public sealed class TrasladoConfiguration : IEntityTypeConfiguration<Traslado>
{
    public void Configure(EntityTypeBuilder<Traslado> b)
    {
        b.ToTable("traslado");
        b.HasKey(x => x.IdTraslado);
        b.Property(x => x.IdTraslado).UseIdentityAlwaysColumn();
        b.Property(x => x.Folio).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Folio).IsUnique();
        b.Property(x => x.IdOrigen).IsRequired();
        b.Property(x => x.IdDestino).IsRequired();
        b.Property(x => x.IdUsuarioSolicita).IsRequired();
        b.Property(x => x.FechaSolicitud).HasDefaultValueSql("now()").IsRequired();
        b.Property(x => x.FechaEnvio);
        b.Property(x => x.FechaRecepcion);
        b.Property(x => x.Estado).HasConversion(new SnakeUpperEnumConverter<EstadoTraslado>()).HasMaxLength(15).HasDefaultValue(EstadoTraslado.Solicitado).IsRequired();
        b.Property(x => x.Observaciones).HasMaxLength(200);
        b.Property(x => x.Xmin).HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        b.HasOne(x => x.Origen).WithMany().HasForeignKey(x => x.IdOrigen);
        b.HasOne(x => x.Destino).WithMany().HasForeignKey(x => x.IdDestino);
        b.HasOne(x => x.UsuarioSolicita).WithMany().HasForeignKey(x => x.IdUsuarioSolicita);
        b.HasCheckConstraint("ck_traslado_origen_destino", "id_origen <> id_destino");
        b.HasIndex(x => x.Estado).HasFilter("estado IN ('SOLICITADO','EN_TRANSITO')");
    }
}