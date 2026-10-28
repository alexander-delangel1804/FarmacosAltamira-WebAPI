using FarmacosAltamira.Application.Caja.Dto;
using FarmacosAltamira.Application.Catalogo.Dto;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Sincronizacion.Dto;
using FarmacosAltamira.Contracts;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Entities.Local;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FarmacosAltamira.Application.Sincronizacion.Services;

public sealed class SincronizacionService
{
    private readonly ILocalDbContext _localDb;
    private readonly ICentralDbContext _centralDb;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SincronizacionService> _logger;
    private readonly IAuthService _authService;

    public SincronizacionService(
        ILocalDbContext localDb,
        ICentralDbContext centralDb,
        IHttpClientFactory httpClientFactory,
        ILogger<SincronizacionService> logger,
        IAuthService authService)
    {
        _localDb = localDb;
        _centralDb = centralDb;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _authService = authService;
    }

    public async Task<SubirVentasResultado> SubirVentasPendientesAsync(CancellationToken ct)
    {
        var ventasPendientes = await _localDb.Ventas
            .Where(v => v.SyncEstado == "PENDIENTE")
            .OrderBy(v => v.IdLocal)
            .Take(50)
            .ToListAsync(ct);

        if (ventasPendientes.Count == 0)
        {
            _logger.LogInformation("No hay ventas pendientes de sincronizar");
            return new SubirVentasResultado(0, 0, 0);
        }

        _logger.LogInformation("Encontradas {Count} ventas pendientes de sincronizar", ventasPendientes.Count);

        var token = await ObtenerTokenCentralAsync(ct);
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("No se pudo obtener token de autenticación para la API central");
            return new SubirVentasResultado(0, 0, ventasPendientes.Count);
        }

        var cliente = _httpClientFactory.CreateClient("CentralApi");
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        int exitosas = 0;
        int fallidas = 0;

        foreach (var venta in ventasPendientes)
        {
            try
            {
                var resultado = await SincronizarVentaAsync(cliente, venta, ct);

                if (resultado.Exito)
                {
                    exitosas++;
                    _logger.LogInformation("Venta {Folio} sincronizada correctamente (id_venta_central: {IdVentaCentral})",
                        venta.Folio, resultado.IdVentaCentral);
                }
                else
                {
                    fallidas++;
                    _logger.LogWarning("Error sincronizando venta {Folio}: {Error}", venta.Folio, resultado.Error);
                }
            }
            catch (Exception ex)
            {
                fallidas++;
                _logger.LogError(ex, "Excepción sincronizando venta {Folio}", venta.Folio);

                venta.SyncEstado = "PENDIENTE";
                venta.SyncIntentos++;
                venta.SyncUltimoError = ex.Message;
                await _localDb.SaveChangesAsync(ct);
            }
        }

        return new SubirVentasResultado(exitosas, fallidas, ventasPendientes.Count);
    }

    private async Task<string?> ObtenerTokenCentralAsync(CancellationToken ct)
    {
        try
        {
            var meta = await _localDb.Meta.FirstOrDefaultAsync(m => m.Clave == "sync_usuario", ct);
            var metaPass = await _localDb.Meta.FirstOrDefaultAsync(m => m.Clave == "sync_password", ct);
            var metaEstacion = await _localDb.Meta.FirstOrDefaultAsync(m => m.Clave == "sync_id_estacion", ct);
            var metaSecreto = await _localDb.Meta.FirstOrDefaultAsync(m => m.Clave == "sync_secreto_estacion", ct);

            if (meta == null || metaPass == null || metaEstacion == null || metaSecreto == null)
            {
                _logger.LogWarning("Credenciales de sincronización no configuradas en meta_local");
                return null;
            }

            var loginRequest = new
            {
                IdEstacion = int.Parse(metaEstacion.Valor),
                SecretoEstacion = metaSecreto.Valor,
                NombreUsuario = meta.Valor,
                Contrasena = metaPass.Valor
            };

            var loginCliente = _httpClientFactory.CreateClient("CentralApi");
            var response = await loginCliente.PostAsJsonAsync("/api/v1/auth/login", loginRequest, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error autenticando en API central: {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(ct);
            return result?.Dato?.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo token central");
            return null;
        }
    }

    private async Task<SincronizarVentaResultado> SincronizarVentaAsync(HttpClient cliente, VentaLocal venta, CancellationToken ct)
    {
        venta.SyncEstado = "ENVIANDO";
        venta.SyncIntentos++;
        await _localDb.SaveChangesAsync(ct);

        try
        {
            var detalles = await _localDb.DetallesVenta
                .Where(d => d.IdLocal == venta.IdLocal)
                .ToListAsync(ct);

            var pagos = await _localDb.Pagos
                .Where(p => p.IdLocal == venta.IdLocal)
                .ToListAsync(ct);

            var idCorteCentral = await ResolverIdCorteCentralAsync(venta.IdCorteLocal, ct);
            if (idCorteCentral == null)
            {
                throw new InvalidOperationException($"No se pudo resolver corte central para corte_local={venta.IdCorteLocal}");
            }

            var payload = new VentaCreateDto(
                UuidOperacion: Guid.Parse(venta.UuidOperacion),
                Folio: venta.Folio,
                IdCorte: idCorteCentral.Value,
                FechaHora: DateTimeOffset.Parse(venta.FechaHora),
                Subtotal: venta.Subtotal,
                Descuento: venta.Descuento,
                Iva: venta.Iva,
                Total: venta.Total,
                RequiereFactura: venta.RequiereFactura,
                Detalles: detalles.Select(d => new DetalleVentaCreateDto(
                    IdProducto: d.IdProducto,
                    IdLote: d.IdLote,
                    IdPromocion: d.IdPromocion,
                    Cantidad: d.Cantidad,
                    PrecioUnitario: d.PrecioUnitario,
                    Descuento: d.Descuento,
                    Importe: d.Importe)).ToList(),
                Pagos: pagos.Select(p => new PagoCreateDto(
                    FormaPago: p.FormaPago,
                    Monto: p.Monto,
                    Referencia: p.Referencia,
                    Cambio: p.Cambio)).ToList(),
                Receta: null);

            var response = await cliente.PostAsJsonAsync("/api/v1/ventas", payload, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var conflictResult = await response.Content.ReadFromJsonAsync<ApiResponse<VentaResultadoDto>>(ct);
                if (conflictResult?.Dato?.Resultado == "YA_RECIBIDA")
                {
                    venta.SyncEstado = "SINCRONIZADA";
                    venta.IdVentaCentral = conflictResult.Dato.IdVenta;
                    venta.SyncFecha = DateTimeOffset.UtcNow.ToString("o");
                    await _localDb.SaveChangesAsync(ct);

                    await MarcarColaComoHechaAsync(venta.UuidOperacion, ct);
                    return new SincronizarVentaResultado(true, conflictResult.Dato.IdVenta, null);
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Error {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<VentaResultadoDto>>(ct);

            if (result?.Exito == true && result.Dato != null)
            {
                venta.SyncEstado = "SINCRONIZADA";
                venta.IdVentaCentral = result.Dato.IdVenta;
                venta.SyncFecha = DateTimeOffset.UtcNow.ToString("o");
                await _localDb.SaveChangesAsync(ct);

                await MarcarColaComoHechaAsync(venta.UuidOperacion, ct);

                return new SincronizarVentaResultado(true, result.Dato.IdVenta, null);
            }

            throw new InvalidOperationException($"Respuesta inesperada: {result?.Mensaje}");
        }
        catch (Exception ex)
        {
            venta.SyncEstado = "PENDIENTE";
            venta.SyncUltimoError = ex.Message;
            await _localDb.SaveChangesAsync(ct);

            return new SincronizarVentaResultado(false, 0, ex.Message);
        }
    }

    private async Task<int?> ResolverIdCorteCentralAsync(int idCorteLocal, CancellationToken ct)
    {
        var metaKey = $"corte_local_{idCorteLocal}_central";
        var meta = await _localDb.Meta.FirstOrDefaultAsync(m => m.Clave == metaKey, ct);
        if (meta != null && int.TryParse(meta.Valor, out var idCentral))
        {
            return idCentral;
        }

        var corteLocal = await _localDb.CortesCaja.FirstOrDefaultAsync(c => c.IdCorteLocal == idCorteLocal, ct);
        if (corteLocal == null)
            return null;

        var metaExistente = await _localDb.Meta.FirstOrDefaultAsync(m => m.Clave == $"corte_local_{idCorteLocal}_central", ct);
        if (metaExistente != null && int.TryParse(metaExistente.Valor, out var id))
        {
            return id;
        }

        return idCorteLocal;
    }

    private async Task MarcarColaComoHechaAsync(string uuidOperacion, CancellationToken ct)
    {
        var cola = await _localDb.ColaSincronizacion
            .FirstOrDefaultAsync(c => c.UuidOperacion == uuidOperacion && c.Tipo == "VENTA", ct);

        if (cola != null)
        {
            cola.Estado = "HECHA";
            await _localDb.SaveChangesAsync(ct);
        }
    }

    public async Task<BajarDatosResultado> BajarDatosAsync(DateTimeOffset? desde, CancellationToken ct)
    {
        var resultado = new BajarDatosResultado();

        // 1. Descargar catálogo de productos
        try
        {
            var (productos, conflictivos) = await DescargarProductosAsync(desde, ct);
            resultado.ProductosDescargados = productos;
            resultado.ProductosConflictivos = conflictivos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error descendiendo catálogo de productos");
            resultado.Errores.Add("ERROR_DESCARGANDO_PRODUCTOS", ex.Message);
        }

        // 2. Descargar usuarios
        try
        {
            var (usuarios, conflictivos) = await DescargarUsuariosAsync(desde, ct);
            resultado.UsuariosDescargados = usuarios;
            resultado.UsuariosConflictivos = conflictivos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error descendiendo usuarios");
            resultado.Errores.Add("ERROR_DESCARGANDO_USUARIOS", ex.Message);
        }

        // 3. Descargar existencias
        try
        {
            var (existencias, conflictivos) = await DescargarExistenciasAsync(desde, ct);
            resultado.ExistenciasDescargadas = existencias;
            resultado.ExistenciasConflictivas = conflictivos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error descendiendo existencias");
            resultado.Errores.Add("ERROR_DESCARGANDO_EXISTENCIAS", ex.Message);
        }

        // 4. Parámetros - no disponibles en local.db aún
        _logger.LogInformation("Parámetros: omitidos (no hay tabla local parametros)");

        return resultado;
    }

    private async Task<(IReadOnlyList<ProductoLocal> Items, int Conflictivos)>
        DescargarProductosAsync(DateTimeOffset? desde, CancellationToken ct)
    {
        var items = await _localDb.Productos
            .Where(p => !desde.HasValue || EF.Property<DateTimeOffset>(p, "FechaActualizacion") >= desde.Value)
            .OrderBy(p => p.NombreComercial)
            .Take(2000)
            .ToListAsync(ct);

        int conflictivos = 0;
        foreach (var producto in items)
        {
            try
            {
                await AplicarProductoAsync(producto, ct);
            }
            catch
            {
                conflictivos++;
            }
        }

        return (items, conflictivos);
    }

    private async Task<(IReadOnlyList<UsuarioLocal> Items, int Conflictivos)>
        DescargarUsuariosAsync(DateTimeOffset? desde, CancellationToken ct)
    {
        var items = await _localDb.Usuarios
            .Where(u => !desde.HasValue || EF.Property<DateTimeOffset>(u, "FechaActualizacion") >= desde.Value)
            .OrderBy(u => u.NombreUsuario)
            .Take(500)
            .ToListAsync(ct);

        int conflictivos = 0;
        foreach (var usuario in items)
        {
            try
            {
                await AplicarUsuarioAsync(usuario, ct);
            }
            catch
            {
                conflictivos++;
            }
        }

        return (items, conflictivos);
    }

    private async Task<(IReadOnlyList<ExistenciaLocal> Items, int Conflictivos)>
        DescargarExistenciasAsync(DateTimeOffset? desde, CancellationToken ct)
    {
        var items = await _localDb.Existencias
            .Where(e => !desde.HasValue || EF.Property<DateTimeOffset>(e, "FechaActualizacion") >= desde.Value)
            .OrderBy(e => e.IdLote)
            .Take(2000)
            .ToListAsync(ct);

        int conflictivos = 0;
        foreach (var existencia in items)
        {
            try
            {
                await AplicarExistenciaAsync(existencia, ct);
            }
            catch
            {
                conflictivos++;
            }
        }

        return (items, conflictivos);
    }

    private async Task AplicarProductoAsync(ProductoLocal producto, CancellationToken ct)
    {
        // Buscar en central por codigo_barras
        var productoCentral = await _centralDb.Productos
            .FirstOrDefaultAsync(p => p.CodigoBarras == producto.CodigoBarras, ct);

        if (productoCentral == null)
        {
            // Crear nuevo registro local con los datos disponibles
            // El productoLocal no tiene FechaActualizacion, así que solo copiamos los campos disponibles
            var nuevo = new ProductoLocal
            {
                CodigoBarras = producto.CodigoBarras,
                NombreComercial = producto.NombreComercial,
                PrecioVenta = producto.PrecioVenta,
                TasaIva = producto.TasaIva,
                EsControlado = producto.EsControlado,
                Activo = producto.Activo
            };
            _localDb.Productos.Add(nuevo);
        }
        else
        {
            // Actualizar local con datos del central (gana el servidor)
            // Copiar solo los campos que existen en ProductoLocal
            if (productoCentral.NombreComercial != producto.NombreComercial)
            {
                producto.NombreComercial = productoCentral.NombreComercial;
            }

            if (productoCentral.PrecioVenta != producto.PrecioVenta)
            {
                producto.PrecioVenta = productoCentral.PrecioVenta;
            }

            if (productoCentral.TasaIva != producto.TasaIva)
            {
                producto.TasaIva = productoCentral.TasaIva;
            }

            if (productoCentral.EsControlado != producto.EsControlado)
            {
                producto.EsControlado = productoCentral.EsControlado;
            }

            if (productoCentral.Activo != producto.Activo)
            {
                producto.Activo = productoCentral.Activo;
            }
        }

        await _localDb.SaveChangesAsync(ct);
    }

    private async Task AplicarUsuarioAsync(UsuarioLocal usuario, CancellationToken ct)
    {
        // Buscar en central por nombre_usuario
        var usuarioCentral = await _centralDb.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == usuario.NombreUsuario, ct);

        if (usuarioCentral == null)
        {
            // Crear nuevo usuario en local
            var nuevo = new UsuarioLocal
            {
                NombreUsuario = usuario.NombreUsuario,
                HashContrasena = usuario.HashContrasena,
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                Bloqueado = usuario.Bloqueado
            };
            _localDb.Usuarios.Add(nuevo);
        }
        else
        {
            // Actualizar local con datos del central (gana el servidor)
            // Copiar solo los campos que existen en UsuarioLocal (Rol es string en local, enum en central)
            if (usuarioCentral.Rol.ToString() != usuario.Rol)
            {
                usuario.Rol = usuarioCentral.Rol.ToString();
            }

            if (usuarioCentral.NombreCompleto != usuario.NombreUsuario) // Usar NombreUsuario como placeholder since UsuarioLocal no tiene NombreCompleto
            {
                // No hay campo NombreCompleto en UsuarioLocal, se omite
            }

            if (usuarioCentral.Activo != usuario.Activo)
            {
                usuario.Activo = usuarioCentral.Activo;
            }

            if (usuarioCentral.Bloqueado != usuario.Bloqueado)
            {
                usuario.Bloqueado = usuarioCentral.Bloqueado;
            }
        }

        await _localDb.SaveChangesAsync(ct);
    }

    private async Task AplicarExistenciaAsync(ExistenciaLocal existencia, CancellationToken ct)
    {
        // Buscar en central por id_lote
        var existenciaCentral = await _localDb.Existencias
            .FirstOrDefaultAsync(e => e.IdLote == existencia.IdLote, ct);

        if (existenciaCentral == null)
        {
            // Crear nueva existencia en local
            var nueva = new ExistenciaLocal
            {
                IdLote = existencia.IdLote,
                Cantidad = existencia.Cantidad
            };
            _localDb.Existencias.Add(nueva);
        }
        else
        {
            // Actualizar cantidad - gana el servidor central
            if (existenciaCentral.Cantidad != existencia.Cantidad)
            {
                existencia.Cantidad = existenciaCentral.Cantidad;
            }
        }

        await _localDb.SaveChangesAsync(ct);
    }
}

public sealed record SubirVentasResultado(int Exitosas, int Fallidas, int Total);

public sealed record SincronizarVentaResultado(bool Exito, int IdVentaCentral, string? Error);

public sealed record BajarDatosResultado
{
    public IReadOnlyList<ProductoLocal>? ProductosDescargados { get; set; }
    public int ProductosConflictivos { get; set; }
    public IReadOnlyList<UsuarioLocal>? UsuariosDescargados { get; set; }
    public int UsuariosConflictivos { get; set; }
    public IReadOnlyList<ExistenciaLocal>? ExistenciasDescargadas { get; set; }
    public int ExistenciasConflictivas { get; set; }
    public Dictionary<string, string> Errores { get; set; } = new();
}

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int IdUsuario,
    string NombreUsuario,
    string NombreCompleto,
    string Rol,
    int? IdUbicacion,
    string? UbicacionNombre,
    int? IdCaja,
    string? CajaNumero,
    DateTimeOffset Expiracion);
