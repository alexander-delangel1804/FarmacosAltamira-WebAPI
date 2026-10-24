namespace FarmacosAltamira.Application.Traslados.Dto;

public sealed record TrasladoDto(
    int IdTraslado,
    string Folio,
    int IdOrigen,
    string OrigenNombre,
    int IdDestino,
    string DestinoNombre,
    int IdUsuarioSolicita,
    string UsuarioSolicitaNombre,
    DateTimeOffset FechaSolicitud,
    DateTimeOffset? FechaEnvio,
    DateTimeOffset? FechaRecepcion,
    string Estado,
    string? Observaciones,
    IReadOnlyList<DetalleTrasladoDto> Detalles);

public sealed record DetalleTrasladoDto(
    int IdLote,
    string ProductoNombre,
    string NumeroLote,
    int CantidadEnviada,
    int? CantidadRecibida);

public sealed record TrasladoCreateDto(
    int IdOrigen,
    int IdDestino,
    string? Observaciones,
    IReadOnlyList<DetalleTrasladoCreateDto> Detalles);

public sealed record DetalleTrasladoCreateDto(
    int IdLote,
    int CantidadEnviada);

public sealed record TrasladoRecibirDto(
    IReadOnlyList<DetalleRecibirDto> Detalles);

public sealed record DetalleRecibirDto(
    int IdLote,
    int CantidadRecibida);