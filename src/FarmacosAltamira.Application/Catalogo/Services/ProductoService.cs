using FarmacosAltamira.Application.Catalogo.Dto;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Common.Dtos;
using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Catalogo.Services;

public sealed class ProductoService
{
    private readonly ICentralDbContext _db;
    private readonly IAuditoria _auditoria;

    public ProductoService(ICentralDbContext db, IAuditoria auditoria)
    {
        _db = db;
        _auditoria = auditoria;
    }

    public async Task<PaginatedResponse<ProductoDto>> ListarAsync(string? busqueda, bool soloActivos, int pagina, int tamano, CancellationToken ct)
    {
        var query = _db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.StockMinimoUbicaciones)
                .ThenInclude(smu => smu.Ubicacion)
            .Where(p => !soloActivos || p.Activo);

        if (!string.IsNullOrEmpty(busqueda))
        {
            var term = busqueda.ToLower();
            query = query.Where(p => p.NombreComercial.ToLower().Contains(term) || p.CodigoBarras.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.NombreComercial)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(p => new ProductoDto(
                p.IdProducto,
                p.IdCategoria,
                p.Categoria.Nombre,
                p.CodigoBarras,
                p.NombreComercial,
                p.SustanciaActiva,
                p.Presentacion,
                p.PrecioVenta,
                p.TasaIva,
                p.StockMinimo,
                p.EsControlado,
                p.Activo,
                p.StockMinimoUbicaciones.Select(smu => new StockMinimoUbicacionDto(smu.IdUbicacion, smu.Ubicacion.Nombre, smu.StockMinimo)).ToList()))
            .ToListAsync(ct);

        return new PaginatedResponse<ProductoDto>(items, total, pagina, tamano);
    }

    public async Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        return await _db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.StockMinimoUbicaciones)
                .ThenInclude(smu => smu.Ubicacion)
            .Where(p => p.IdProducto == id)
            .Select(p => new ProductoDto(
                p.IdProducto,
                p.IdCategoria,
                p.Categoria.Nombre,
                p.CodigoBarras,
                p.NombreComercial,
                p.SustanciaActiva,
                p.Presentacion,
                p.PrecioVenta,
                p.TasaIva,
                p.StockMinimo,
                p.EsControlado,
                p.Activo,
                p.StockMinimoUbicaciones.Select(smu => new StockMinimoUbicacionDto(smu.IdUbicacion, smu.Ubicacion.Nombre, smu.StockMinimo)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProductoDto> CrearAsync(ProductoCreateDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        if (await _db.Productos.AnyAsync(p => p.CodigoBarras == dto.CodigoBarras, ct))
            throw new InvalidOperationException("El código de barras ya existe");

        var producto = new Producto
        {
            IdCategoria = dto.IdCategoria,
            CodigoBarras = dto.CodigoBarras,
            NombreComercial = dto.NombreComercial,
            SustanciaActiva = dto.SustanciaActiva,
            Presentacion = dto.Presentacion,
            PrecioVenta = dto.PrecioVenta,
            TasaIva = dto.TasaIva,
            StockMinimo = dto.StockMinimo,
            EsControlado = dto.EsControlado,
            Activo = true
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync(ct);

        // Stock mínimo por ubicación
        foreach (var smu in dto.StockMinimoUbicaciones)
        {
            _db.StockMinimoUbicaciones.Add(new StockMinimoUbicacion
            {
                IdProducto = producto.IdProducto,
                IdUbicacion = smu.IdUbicacion,
                StockMinimo = smu.StockMinimo
            });
        }
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, null, "BODEGA", "CREAR_PRODUCTO", "producto", producto.IdProducto.ToString(), $"Producto creado: {producto.NombreComercial}", cancellationToken: ct);

        return await ObtenerPorIdAsync(producto.IdProducto, ct) ?? throw new InvalidOperationException("Error al crear producto");
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ProductoUpdateDto dto, int idUsuarioAudit, CancellationToken ct)
    {
        var producto = await _db.Productos
            .Include(p => p.StockMinimoUbicaciones)
            .FirstOrDefaultAsync(p => p.IdProducto == id, ct);

        if (producto == null) throw new KeyNotFoundException("Producto no encontrado");

        producto.IdCategoria = dto.IdCategoria;
        producto.NombreComercial = dto.NombreComercial;
        producto.SustanciaActiva = dto.SustanciaActiva;
        producto.Presentacion = dto.Presentacion;
        producto.PrecioVenta = dto.PrecioVenta;
        producto.TasaIva = dto.TasaIva;
        producto.StockMinimo = dto.StockMinimo;
        producto.EsControlado = dto.EsControlado;

        // Actualizar stock mínimo por ubicación
        var existentes = producto.StockMinimoUbicaciones.ToDictionary(s => s.IdUbicacion);
        foreach (var smu in dto.StockMinimoUbicaciones)
        {
            if (existentes.TryGetValue(smu.IdUbicacion, out var actual))
            {
                actual.StockMinimo = smu.StockMinimo;
                existentes.Remove(smu.IdUbicacion);
            }
            else
            {
                _db.StockMinimoUbicaciones.Add(new StockMinimoUbicacion
                {
                    IdProducto = id,
                    IdUbicacion = smu.IdUbicacion,
                    StockMinimo = smu.StockMinimo
                });
            }
        }

        foreach (var remover in existentes.Values)
        {
            _db.StockMinimoUbicaciones.Remove(remover);
        }

        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, null, "BODEGA", "ACTUALIZAR_PRODUCTO", "producto", id.ToString(), $"Producto actualizado: {producto.NombreComercial}", cancellationToken: ct);

        return await ObtenerPorIdAsync(id, ct) ?? throw new InvalidOperationException("Error al actualizar producto");
    }

    public async Task ActualizarStockMinimoAsync(int idProducto, int idUbicacion, int stockMinimo, int idUsuarioAudit, CancellationToken ct)
    {
        var smu = await _db.StockMinimoUbicaciones
            .FirstOrDefaultAsync(s => s.IdProducto == idProducto && s.IdUbicacion == idUbicacion, ct);

        if (smu == null)
        {
            _db.StockMinimoUbicaciones.Add(new StockMinimoUbicacion
            {
                IdProducto = idProducto,
                IdUbicacion = idUbicacion,
                StockMinimo = stockMinimo
            });
        }
        else
        {
            smu.StockMinimo = stockMinimo;
        }

        await _db.SaveChangesAsync(ct);
        await _auditoria.RegistrarAsync(idUsuarioAudit, idUbicacion, "BODEGA", "ACTUALIZAR_STOCK_MINIMO", "stock_minimo_ubicacion", $"{idProducto}-{idUbicacion}", $"Stock mínimo actualizado a {stockMinimo}", cancellationToken: ct);
    }

    public async Task DesactivarAsync(int id, int idUsuarioAudit, CancellationToken ct)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id, ct);
        if (producto == null) throw new KeyNotFoundException("Producto no encontrado");

        producto.Activo = false;
        await _db.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(idUsuarioAudit, null, "BODEGA", "DESACTIVAR_PRODUCTO", "producto", id.ToString(), $"Producto desactivado: {producto.NombreComercial}", cancellationToken: ct);
    }
}