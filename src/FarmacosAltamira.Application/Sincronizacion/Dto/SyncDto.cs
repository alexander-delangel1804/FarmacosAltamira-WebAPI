namespace FarmacosAltamira.Application.Sincronizacion.Dto;

public sealed record SyncProductoDto(
    int IdProducto,
    string CodigoBarras,
    string NombreComercial,
    decimal PrecioVenta,
    decimal TasaIva,
    bool EsControlado,
    bool Activo);

public sealed record SyncPromocionDto(
    int IdPromocion,
    string Nombre,
    string Tipo,
    decimal Valor,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    bool Activa);

public sealed record SyncUsuarioDto(
    int IdUsuario,
    string NombreUsuario,
    string HashContrasena,
    string Rol,
    bool Activo,
    bool Bloqueado,
    DateTimeOffset ActualizadoEn);

public sealed record SyncExistenciaDto(
    int IdLote,
    int Cantidad,
    int CantidadReservada);

public sealed record SyncParametroDto(
    string Clave,
    string Valor);