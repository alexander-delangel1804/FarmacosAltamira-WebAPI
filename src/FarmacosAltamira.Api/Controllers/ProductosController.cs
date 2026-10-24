using FarmacosAltamira.Application.Catalogo.Dto;
using FarmacosAltamira.Application.Catalogo.Services;
using FarmacosAltamira.Api.Extensions;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FarmacosAltamira.Api.Controllers;

[ApiController]
[Route("api/v1/productos")]
[Authorize(Policy = "BODEGA:PRODUCTOS")]
public sealed class ProductosController : ControllerBase
{
    private readonly ProductoService _productoService;

    public ProductosController(ProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductoDto>>>> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] bool soloActivos = true,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await _productoService.ListarAsync(busqueda, soloActivos, pagina, tamano, ct);
        return Ok(ApiResponse<PaginatedResponse<ProductoDto>>.Ok(result.ToContract()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> Obtener(int id, CancellationToken ct)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id, ct);
        return producto == null
            ? NotFound(ApiResponse<ProductoDto>.Fail("Producto no encontrado", "NO_ENCONTRADO"))
            : Ok(ApiResponse<ProductoDto>.Ok(producto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> Crear([FromBody] ProductoCreateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var producto = await _productoService.CrearAsync(dto, userId, ct);
        return CreatedAtAction(nameof(Obtener), new { id = producto.IdProducto }, ApiResponse<ProductoDto>.Ok(producto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductoDto>>> Actualizar(int id, [FromBody] ProductoUpdateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var producto = await _productoService.ActualizarAsync(id, dto, userId, ct);
        return Ok(ApiResponse<ProductoDto>.Ok(producto));
    }

    [HttpPut("{id:int}/stock-minimo/{idUbicacion:int}")]
    public async Task<ActionResult<ApiResponse<object>>> ActualizarStockMinimo(int id, int idUbicacion, [FromBody] StockMinimoUpdateDto dto, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _productoService.ActualizarStockMinimoAsync(id, idUbicacion, dto.StockMinimo, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Stock mínimo actualizado"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Desactivar(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        await _productoService.DesactivarAsync(id, userId, ct);
        return Ok(ApiResponse<object>.Ok(null, "Producto desactivado"));
    }
}

[ApiController]
[Route("api/v1/categorias")]
[Authorize(Policy = "BODEGA:PRODUCTOS")]
public sealed class CategoriasController : ControllerBase
{
    private readonly CategoriaService _categoriaService;

    public CategoriasController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoriaDto>>>> Listar(CancellationToken ct)
    {
        var categorias = await _categoriaService.ListarAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CategoriaDto>>.Ok(categorias));
    }
}