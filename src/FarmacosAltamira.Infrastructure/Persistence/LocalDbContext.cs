using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities.Local;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FarmacosAltamira.Infrastructure.Persistence;

public sealed class LocalDbContext : DbContext, ILocalDbContext
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

    public DbSet<ProductoLocal> Productos { get; set; } = null!;
    public DbSet<LoteLocal> Lotes { get; set; } = null!;
    public DbSet<ExistenciaLocal> Existencias { get; set; } = null!;
    public DbSet<PromocionLocal> Promociones { get; set; } = null!;
    public DbSet<UsuarioLocal> Usuarios { get; set; } = null!;
    public DbSet<CorteCajaLocal> CortesCaja { get; set; } = null!;
    public DbSet<VentaLocal> Ventas { get; set; } = null!;
    public DbSet<DetalleVentaLocal> DetallesVenta { get; set; } = null!;
    public DbSet<PagoLocal> Pagos { get; set; } = null!;
    public DbSet<RecetaLocal> Recetas { get; set; } = null!;
    public DbSet<RegistroControladoLocal> RegistrosControlados { get; set; } = null!;
    public DbSet<ColaSincronizacion> ColaSincronizacion { get; set; } = null!;
    public DbSet<MetaLocal> Meta { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar nombres de tabla exactos (snake_case en BD)
        modelBuilder.Entity<ProductoLocal>().ToTable("producto_local");
        modelBuilder.Entity<LoteLocal>().ToTable("lote_local");
        modelBuilder.Entity<ExistenciaLocal>().ToTable("existencia_local");
        modelBuilder.Entity<PromocionLocal>().ToTable("promocion_local");
        modelBuilder.Entity<UsuarioLocal>().ToTable("usuario_local");
        modelBuilder.Entity<CorteCajaLocal>().ToTable("corte_caja_local");
        modelBuilder.Entity<VentaLocal>().ToTable("venta_local");
        modelBuilder.Entity<DetalleVentaLocal>().ToTable("detalle_venta_local");
        modelBuilder.Entity<PagoLocal>().ToTable("pago_local");
        modelBuilder.Entity<RecetaLocal>().ToTable("receta_local");
        modelBuilder.Entity<RegistroControladoLocal>().ToTable("registro_controlado_local");
        modelBuilder.Entity<ColaSincronizacion>().ToTable("cola_sincronizacion");
        modelBuilder.Entity<MetaLocal>().ToTable("meta_local");

        // PKs
        modelBuilder.Entity<ProductoLocal>().HasKey(e => e.IdProducto);
        modelBuilder.Entity<LoteLocal>().HasKey(e => e.IdLote);
        modelBuilder.Entity<ExistenciaLocal>().HasKey(e => e.IdLote);
        modelBuilder.Entity<PromocionLocal>().HasKey(e => e.IdPromocion);
        modelBuilder.Entity<UsuarioLocal>().HasKey(e => e.IdUsuario);
        modelBuilder.Entity<CorteCajaLocal>().HasKey(e => e.IdCorteLocal);
        modelBuilder.Entity<VentaLocal>().HasKey(e => e.IdLocal);
        modelBuilder.Entity<DetalleVentaLocal>().HasKey(e => e.IdDetalleLocal);
        modelBuilder.Entity<PagoLocal>().HasKey(e => e.IdPagoLocal);
        modelBuilder.Entity<RecetaLocal>().HasKey(e => e.IdRecetaLocal);
        modelBuilder.Entity<RegistroControladoLocal>().HasKey(e => e.IdRegistroLocal);
        modelBuilder.Entity<ColaSincronizacion>().HasKey(e => e.IdCola);
        modelBuilder.Entity<MetaLocal>().HasKey(e => e.Clave);

        // Índices únicos
        modelBuilder.Entity<ProductoLocal>()
            .HasIndex(e => e.CodigoBarras)
            .IsUnique()
            .HasDatabaseName("ux_producto_local_codigo_barras");

        modelBuilder.Entity<VentaLocal>()
            .HasIndex(e => e.UuidOperacion)
            .IsUnique()
            .HasDatabaseName("ux_venta_local_uuid_operacion");

        modelBuilder.Entity<VentaLocal>()
            .HasIndex(e => e.Folio)
            .IsUnique()
            .HasDatabaseName("ux_venta_local_folio");

        modelBuilder.Entity<ColaSincronizacion>()
            .HasIndex(e => e.UuidOperacion)
            .IsUnique()
            .HasDatabaseName("ux_cola_sincronizacion_uuid_operacion");

        modelBuilder.Entity<UsuarioLocal>()
            .HasIndex(e => e.NombreUsuario)
            .IsUnique()
            .HasDatabaseName("ux_usuario_local_nombre_usuario");

        // Índices para consultas de sincronización
        modelBuilder.Entity<ColaSincronizacion>()
            .HasIndex(c => new { c.Estado, c.ProximoIntento })
            .HasDatabaseName("ix_cola_pendientes");

        modelBuilder.Entity<VentaLocal>()
            .HasIndex(v => v.SyncEstado)
            .HasDatabaseName("ix_venta_local_sync");

        // Conversión bool ↔ int (SQLite usa INTEGER 0/1)
        var boolProperties = new[]
        {
            typeof(ProductoLocal).GetProperty(nameof(ProductoLocal.EsControlado)),
            typeof(ProductoLocal).GetProperty(nameof(ProductoLocal.Activo)),
            typeof(UsuarioLocal).GetProperty(nameof(UsuarioLocal.Activo)),
            typeof(UsuarioLocal).GetProperty(nameof(UsuarioLocal.Bloqueado)),
            typeof(PromocionLocal).GetProperty(nameof(PromocionLocal.Activa)),
            typeof(RecetaLocal).GetProperty(nameof(RecetaLocal.Retenida)),
        };

        foreach (var prop in boolProperties)
        {
            if (prop != null)
            {
                modelBuilder.Entity(prop.DeclaringType!)
                    .Property(prop.Name)
                    .HasConversion<int>();
            }
        }

        // Configurar strings con longitud máxima donde aplique
        modelBuilder.Entity<ProductoLocal>()
            .Property(e => e.CodigoBarras)
            .HasMaxLength(20);

        modelBuilder.Entity<ProductoLocal>()
            .Property(e => e.NombreComercial)
            .HasMaxLength(120);

        modelBuilder.Entity<LoteLocal>()
            .Property(e => e.NumeroLote)
            .HasMaxLength(30);

        modelBuilder.Entity<LoteLocal>()
            .Property(e => e.FechaCaducidad)
            .HasMaxLength(10); // YYYY-MM-DD

        modelBuilder.Entity<UsuarioLocal>()
            .Property(e => e.NombreUsuario)
            .HasMaxLength(50);

        modelBuilder.Entity<UsuarioLocal>()
            .Property(e => e.HashContrasena)
            .HasMaxLength(255);

        modelBuilder.Entity<UsuarioLocal>()
            .Property(e => e.Rol)
            .HasMaxLength(40);

        modelBuilder.Entity<VentaLocal>()
            .Property(e => e.UuidOperacion)
            .HasMaxLength(36);

        modelBuilder.Entity<VentaLocal>()
            .Property(e => e.Folio)
            .HasMaxLength(20);

        modelBuilder.Entity<VentaLocal>()
            .Property(e => e.SyncEstado)
            .HasMaxLength(20);

        modelBuilder.Entity<ColaSincronizacion>()
            .Property(e => e.UuidOperacion)
            .HasMaxLength(36);

        modelBuilder.Entity<ColaSincronizacion>()
            .Property(e => e.Tipo)
            .HasMaxLength(20);

        modelBuilder.Entity<ColaSincronizacion>()
            .Property(e => e.Estado)
            .HasMaxLength(20);

        modelBuilder.Entity<MetaLocal>()
            .Property(e => e.Clave)
            .HasMaxLength(60);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}