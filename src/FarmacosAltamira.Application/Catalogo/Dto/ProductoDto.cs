namespace FarmacosAltamira.Application.Catalogo.Dto;

public sealed record ProductoDto(
    int IdProducto,
    int IdCategoria,
    string CategoriaNombre,
    string CodigoBarras,
    string NombreComercial,
    string? SustanciaActiva,
    string? Presentacion,
    decimal PrecioVenta,
    decimal TasaIva,
    int StockMinimo,
    bool EsControlado,
    bool Activo,
    IReadOnlyList<StockMinimoUbicacionDto> StockMinimoUbicaciones);

public sealed record StockMinimoUbicacionDto(
    int IdUbicacion,
    string UbicacionNombre,
    int StockMinimo);

public sealed record ProductoCreateDto(
    int IdCategoria,
    string CodigoBarras,
    string NombreComercial,
    string? SustanciaActiva,
    string? Presentacion,
    decimal PrecioVenta,
    decimal TasaIva,
    int StockMinimo,
    bool EsControlado,
    IReadOnlyList<StockMinimoUbicacionCreateDto> StockMinimoUbicaciones);

public sealed record StockMinimoUbicacionCreateDto(
    int IdUbicacion,
    int StockMinimo);

public sealed record ProductoUpdateDto(
    int IdCategoria,
    string NombreComercial,
    string? SustanciaActiva,
    string? Presentacion,
    decimal PrecioVenta,
    decimal TasaIva,
    int StockMinimo,
    bool EsControlado,
    IReadOnlyList<StockMinimoUbicacionCreateDto> StockMinimoUbicaciones);

public sealed record StockMinimoUpdateDto(
    int StockMinimo);