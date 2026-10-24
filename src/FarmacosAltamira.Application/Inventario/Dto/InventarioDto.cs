namespace FarmacosAltamira.Application.Inventario.Dto;

public sealed record LoteDto(
    int IdLote,
    int IdProducto,
    string ProductoNombre,
    string CodigoBarras,
    string NumeroLote,
    DateOnly FechaCaducidad,
    DateOnly FechaIngreso,
    string Estado);

public sealed record ExistenciaDto(
    int IdProducto,
    string ProductoNombre,
    string CodigoBarras,
    int IdLote,
    string NumeroLote,
    DateOnly FechaCaducidad,
    int IdUbicacion,
    string UbicacionNombre,
    int Cantidad,
    int CantidadReservada,
    int Disponible);

public sealed record MovimientoDto(
    long IdMovimiento,
    int IdLote,
    string ProductoNombre,
    string NumeroLote,
    int IdUbicacion,
    string UbicacionNombre,
    string UsuarioNombre,
    DateTimeOffset FechaHora,
    string Tipo,
    int Cantidad,
    string? Motivo,
    string? DocumentoOrigen,
    long? IdDocumento);

public sealed record RecepcionCreateDto(
    int IdUbicacion,
    string? DocumentoReferencia,
    string? Observaciones,
    IReadOnlyList<DetalleRecepcionCreateDto> Detalles);

public sealed record DetalleRecepcionCreateDto(
    int IdProducto,
    string NumeroLote,
    DateOnly FechaCaducidad,
    int Cantidad);

public sealed record RecepcionDto(
    int IdRecepcion,
    int IdUbicacion,
    string UbicacionNombre,
    int IdUsuario,
    string UsuarioNombre,
    DateTimeOffset FechaHora,
    string? DocumentoReferencia,
    string? Observaciones,
    IReadOnlyList<DetalleRecepcionDto> Detalles);

public sealed record DetalleRecepcionDto(
    int IdLote,
    string ProductoNombre,
    string NumeroLote,
    int CantidadRecibida);

public sealed record AjusteCreateDto(
    int IdLote,
    int IdUbicacion,
    int Cantidad,
    string Motivo);

public sealed record AlertaDto(
    int IdAlerta,
    int IdProducto,
    string ProductoNombre,
    int IdUbicacion,
    string UbicacionNombre,
    int? IdLote,
    string? NumeroLote,
    string Tipo,
    DateTimeOffset FechaGeneracion,
    string? Mensaje,
    bool Atendida);