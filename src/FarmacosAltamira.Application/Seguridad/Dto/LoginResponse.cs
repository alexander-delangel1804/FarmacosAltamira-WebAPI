namespace FarmacosAltamira.Application.Seguridad.Dto;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int IdUsuario,
    string NombreUsuario,
    string NombreCompleto,
    string Rol,
    int? IdUbicacion,
    string UbicacionNombre,
    int? IdCaja,
    string? CajaNumero,
    DateTimeOffset Expiracion);