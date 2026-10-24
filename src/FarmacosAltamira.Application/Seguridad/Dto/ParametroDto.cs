namespace FarmacosAltamira.Application.Seguridad.Dto;

public sealed record ParametroDto(
    string Clave,
    string Valor,
    string? Descripcion,
    DateTimeOffset FechaModificacion);

public sealed record ParametroUpdateDto(
    string Valor);