using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Infrastructure.Persistence.Configurations;
using FarmacosAltamira.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace FarmacosAltamira.Infrastructure.Persistence;

public sealed class CentralDbContext : DbContext, ICentralDbContext
{
    public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options) { }

    // Seguridad y Organización
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<BitacoraEvento> BitacoraEventos => Set<BitacoraEvento>();
    public DbSet<Respaldo> Respaldos => Set<Respaldo>();
    public DbSet<ParametroSistema> Parametros => Set<ParametroSistema>();
    public DbSet<Estacion> Estaciones => Set<Estacion>();
    public DbSet<SesionUsuario> SesionesUsuario => Set<SesionUsuario>();

    // Catálogo e Inventario
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<StockMinimoUbicacion> StockMinimoUbicaciones => Set<StockMinimoUbicacion>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<RecepcionMercancia> RecepcionesMercancia => Set<RecepcionMercancia>();
    public DbSet<DetalleRecepcion> DetallesRecepcion => Set<DetalleRecepcion>();
    public DbSet<Traslado> Traslados => Set<Traslado>();
    public DbSet<DetalleTraslado> DetallesTraslado => Set<DetalleTraslado>();
    public DbSet<AlertaInventario> AlertasInventario => Set<AlertaInventario>();

    // Caja y Ventas
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<CorteCaja> CortesCaja => Set<CorteCaja>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<DetalleDevolucion> DetallesDevolucion => Set<DetalleDevolucion>();
    public DbSet<CancelacionVenta> CancelacionesVenta => Set<CancelacionVenta>();

    // Facturación
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ComprobanteFiscal> ComprobantesFiscales => Set<ComprobanteFiscal>();
    public DbSet<EmisorFiscal> EmisoresFiscales => Set<EmisorFiscal>();
    public DbSet<ProductoFiscal> ProductosFiscales => Set<ProductoFiscal>();
    public DbSet<ComprobanteFiscalEvento> ComprobanteFiscalEventos => Set<ComprobanteFiscalEvento>();

    // Pago de Servicios
    public DbSet<CatalogoServicio> CatalogoServicios => Set<CatalogoServicio>();
    public DbSet<ProveedorPagoServicio> ProveedoresPagoServicio => Set<ProveedorPagoServicio>();
    public DbSet<PagoServicio> PagosServicio => Set<PagoServicio>();
    public DbSet<PagoServicioEvento> PagosServicioEventos => Set<PagoServicioEvento>();

    // Controlados
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<RegistroControlado> RegistrosControlados => Set<RegistroControlado>();

    // ICentralDbContext explicit implementation
    DatabaseFacade ICentralDbContext.Database => Database;

    Task<int> ICentralDbContext.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    Task<IDbContextTransaction> ICentralDbContext.BeginTransactionAsync(CancellationToken cancellationToken) => Database.BeginTransactionAsync(cancellationToken);

    Task ICentralDbContext.CommitTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = Database.CurrentTransaction;
        if (transaction != null)
        {
            return transaction.CommitAsync(cancellationToken);
        }
        return Task.CompletedTask;
    }

    Task ICentralDbContext.RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = Database.CurrentTransaction;
        if (transaction != null)
        {
            return transaction.RollbackAsync(cancellationToken);
        }
        return Task.CompletedTask;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.UseIdentityAlwaysColumns();

        // Configuraciones
        modelBuilder.ApplyConfiguration(new UbicacionConfiguration());
        modelBuilder.ApplyConfiguration(new RolConfiguration());
        modelBuilder.ApplyConfiguration(new PermisoConfiguration());
        modelBuilder.ApplyConfiguration(new RolPermisoConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new BitacoraEventoConfiguration());
        modelBuilder.ApplyConfiguration(new RespaldoConfiguration());
        modelBuilder.ApplyConfiguration(new ParametroSistemaConfiguration());
        modelBuilder.ApplyConfiguration(new EstacionConfiguration());
        modelBuilder.ApplyConfiguration(new SesionUsuarioConfiguration());

        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new ProductoConfiguration());
        modelBuilder.ApplyConfiguration(new StockMinimoUbicacionConfiguration());
        modelBuilder.ApplyConfiguration(new LoteConfiguration());
        modelBuilder.ApplyConfiguration(new ExistenciaConfiguration());
        modelBuilder.ApplyConfiguration(new MovimientoInventarioConfiguration());
        modelBuilder.ApplyConfiguration(new RecepcionMercanciaConfiguration());
        modelBuilder.ApplyConfiguration(new DetalleRecepcionConfiguration());
        modelBuilder.ApplyConfiguration(new TrasladoConfiguration());
        modelBuilder.ApplyConfiguration(new DetalleTrasladoConfiguration());
        modelBuilder.ApplyConfiguration(new AlertaInventarioConfiguration());

        modelBuilder.ApplyConfiguration(new CajaConfiguration());
        modelBuilder.ApplyConfiguration(new CorteCajaConfiguration());
        modelBuilder.ApplyConfiguration(new PromocionConfiguration());
        modelBuilder.ApplyConfiguration(new VentaConfiguration());
        modelBuilder.ApplyConfiguration(new DetalleVentaConfiguration());
        modelBuilder.ApplyConfiguration(new PagoConfiguration());
        modelBuilder.ApplyConfiguration(new DevolucionConfiguration());
        modelBuilder.ApplyConfiguration(new DetalleDevolucionConfiguration());
        modelBuilder.ApplyConfiguration(new CancelacionVentaConfiguration());

        modelBuilder.ApplyConfiguration(new ClienteConfiguration());
        modelBuilder.ApplyConfiguration(new ComprobanteFiscalConfiguration());
        modelBuilder.ApplyConfiguration(new EmisorFiscalConfiguration());
        modelBuilder.ApplyConfiguration(new ProductoFiscalConfiguration());
        modelBuilder.ApplyConfiguration(new ComprobanteFiscalEventoConfiguration());

        modelBuilder.ApplyConfiguration(new CatalogoServicioConfiguration());
        modelBuilder.ApplyConfiguration(new ProveedorPagoServicioConfiguration());
        modelBuilder.ApplyConfiguration(new PagoServicioConfiguration());
        modelBuilder.ApplyConfiguration(new PagoServicioEventoConfiguration());

        modelBuilder.ApplyConfiguration(new RecetaConfiguration());
        modelBuilder.ApplyConfiguration(new RegistroControladoConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}