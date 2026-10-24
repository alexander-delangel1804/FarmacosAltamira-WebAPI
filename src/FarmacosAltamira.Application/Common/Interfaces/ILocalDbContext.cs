using FarmacosAltamira.Domain.Entities.Local;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface ILocalDbContext
{
    DbSet<ProductoLocal> Productos { get; }
    DbSet<LoteLocal> Lotes { get; }
    DbSet<ExistenciaLocal> Existencias { get; }
    DbSet<PromocionLocal> Promociones { get; }
    DbSet<UsuarioLocal> Usuarios { get; }
    DbSet<CorteCajaLocal> CortesCaja { get; }
    DbSet<VentaLocal> Ventas { get; }
    DbSet<DetalleVentaLocal> DetallesVenta { get; }
    DbSet<PagoLocal> Pagos { get; }
    DbSet<RecetaLocal> Recetas { get; }
    DbSet<RegistroControladoLocal> RegistrosControlados { get; }
    DbSet<ColaSincronizacion> ColaSincronizacion { get; }
    DbSet<MetaLocal> Meta { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}