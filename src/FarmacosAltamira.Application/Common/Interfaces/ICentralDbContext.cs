using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface ICentralDbContext
{
    DbSet<FarmacosAltamira.Domain.Entities.Ubicacion> Ubicaciones { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Rol> Roles { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Permiso> Permisos { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Usuario> Usuarios { get; }
    DbSet<FarmacosAltamira.Domain.Entities.BitacoraEvento> BitacoraEventos { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Respaldo> Respaldos { get; }
    DbSet<FarmacosAltamira.Domain.Entities.ParametroSistema> Parametros { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Estacion> Estaciones { get; }
    DbSet<FarmacosAltamira.Domain.Entities.SesionUsuario> SesionesUsuario { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Categoria> Categorias { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Producto> Productos { get; }
    DbSet<FarmacosAltamira.Domain.Entities.StockMinimoUbicacion> StockMinimoUbicaciones { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Lote> Lotes { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Existencia> Existencias { get; }
    DbSet<FarmacosAltamira.Domain.Entities.MovimientoInventario> MovimientosInventario { get; }
    DbSet<FarmacosAltamira.Domain.Entities.RecepcionMercancia> RecepcionesMercancia { get; }
    DbSet<FarmacosAltamira.Domain.Entities.DetalleRecepcion> DetallesRecepcion { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Traslado> Traslados { get; }
    DbSet<FarmacosAltamira.Domain.Entities.DetalleTraslado> DetallesTraslado { get; }
    DbSet<FarmacosAltamira.Domain.Entities.AlertaInventario> AlertasInventario { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Caja> Cajas { get; }
    DbSet<FarmacosAltamira.Domain.Entities.CorteCaja> CortesCaja { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Promocion> Promociones { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Venta> Ventas { get; }
    DbSet<FarmacosAltamira.Domain.Entities.DetalleVenta> DetallesVenta { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Pago> Pagos { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Devolucion> Devoluciones { get; }
    DbSet<FarmacosAltamira.Domain.Entities.DetalleDevolucion> DetallesDevolucion { get; }
    DbSet<FarmacosAltamira.Domain.Entities.CancelacionVenta> CancelacionesVenta { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Cliente> Clientes { get; }
    DbSet<FarmacosAltamira.Domain.Entities.ComprobanteFiscal> ComprobantesFiscales { get; }
    DbSet<FarmacosAltamira.Domain.Entities.EmisorFiscal> EmisoresFiscales { get; }
    DbSet<FarmacosAltamira.Domain.Entities.ProductoFiscal> ProductosFiscales { get; }
    DbSet<FarmacosAltamira.Domain.Entities.ComprobanteFiscalEvento> ComprobanteFiscalEventos { get; }
    DbSet<FarmacosAltamira.Domain.Entities.CatalogoServicio> CatalogoServicios { get; }
    DbSet<FarmacosAltamira.Domain.Entities.ProveedorPagoServicio> ProveedoresPagoServicio { get; }
    DbSet<FarmacosAltamira.Domain.Entities.PagoServicio> PagosServicio { get; }
    DbSet<FarmacosAltamira.Domain.Entities.PagoServicioEvento> PagosServicioEventos { get; }
    DbSet<FarmacosAltamira.Domain.Entities.Receta> Recetas { get; }
    DbSet<FarmacosAltamira.Domain.Entities.RegistroControlado> RegistrosControlados { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}