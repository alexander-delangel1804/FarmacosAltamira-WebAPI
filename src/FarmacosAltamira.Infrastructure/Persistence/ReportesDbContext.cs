using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Infrastructure.Persistence;

public sealed class ReportesDbContext : DbContext
{
    public ReportesDbContext(DbContextOptions<ReportesDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // Vistas para reportes
    public DbSet<VentaDiariaView> VentasDiarias => Set<VentaDiariaView>();
    public DbSet<ProductoBajoMinimoView> ProductosBajoMinimo => Set<ProductoBajoMinimoView>();
    public DbSet<TrasladoPendienteView> TrasladosPendientes => Set<TrasladoPendienteView>();
    public DbSet<AlertaAbiertaView> AlertasAbiertas => Set<AlertaAbiertaView>();
    public DbSet<LoteEstadoView> LotesEstado => Set<LoteEstadoView>();
    public DbSet<ProductoStockView> ProductosStock => Set<ProductoStockView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<VentaDiariaView>(e =>
        {
            e.HasNoKey();
            e.ToView("v_venta_diaria");
            e.Property(v => v.Fecha).HasColumnType("date");
            e.Property(v => v.TotalVentas).HasPrecision(12, 2);
        });

        modelBuilder.Entity<ProductoBajoMinimoView>(e =>
        {
            e.HasNoKey();
            e.ToView("v_producto_bajo_minimo");
            e.Property(v => v.PrecioVenta).HasPrecision(12, 2);
        });

        modelBuilder.Entity<TrasladoPendienteView>(e =>
        {
            e.HasNoKey();
            e.ToView("v_traslado_pendiente");
        });

        modelBuilder.Entity<AlertaAbiertaView>(e =>
        {
            e.HasNoKey();
            e.ToView("v_alerta_abierta");
        });

        modelBuilder.Entity<LoteEstadoView>(e =>
        {
            e.HasNoKey();
            e.ToView("v_lote_estado");
        });

        modelBuilder.Entity<ProductoStockView>(e =>
        {
            e.HasNoKey();
            e.ToView("v_producto_stock");
            e.Property(v => v.PrecioVenta).HasPrecision(12, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}

public sealed class VentaDiariaView
{
    public DateOnly Fecha { get; set; }
    public int IdUbicacion { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;
    public int TotalVentas { get; set; }
    public decimal TotalVentasMonto { get; set; }
    public int VentasFacturadas { get; set; }
    public int VentasPendientesFactura { get; set; }
}

public sealed class ProductoBajoMinimoView
{
    public int IdProducto { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public int IdUbicacion { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;
    public int StockMinimo { get; set; }
    public int ExistenciaActual { get; set; }
    public decimal PrecioVenta { get; set; }
}

public sealed class TrasladoPendienteView
{
    public int IdTraslado { get; set; }
    public string Folio { get; set; } = string.Empty;
    public int IdOrigen { get; set; }
    public string OrigenNombre { get; set; } = string.Empty;
    public int IdDestino { get; set; }
    public string DestinoNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTimeOffset FechaSolicitud { get; set; }
}

public sealed class AlertaAbiertaView
{
    public int IdAlerta { get; set; }
    public int IdProducto { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int IdUbicacion { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public DateTimeOffset FechaGeneracion { get; set; }
}

public sealed class LoteEstadoView
{
    public int IdLote { get; set; }
    public int IdProducto { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly FechaCaducidad { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int IdUbicacion { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;
    public int Existencia { get; set; }
}

public sealed class ProductoStockView
{
    public int IdProducto { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public int IdUbicacion { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;
    public int Existencia { get; set; }
    public int StockMinimo { get; set; }
    public decimal PrecioVenta { get; set; }
}