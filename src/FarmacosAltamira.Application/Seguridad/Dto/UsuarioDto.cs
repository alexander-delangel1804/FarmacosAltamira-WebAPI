namespace FarmacosAltamira.Application.Seguridad.Dto;

public sealed record UsuarioDto(
    int IdUsuario,
    string NombreUsuario,
    string NombreCompleto,
    string Rol,
    int? IdUbicacion,
    string? UbicacionNombre,
    bool Activo,
    bool Bloqueado,
    int IntentosFallidos,
    DateTimeOffset? UltimoAcceso,
    DateOnly FechaAlta);

public sealed record UsuarioCreateDto(
    string NombreUsuario,
    string NombreCompleto,
    string Contrasena,
    int IdRol,
    int? IdUbicacion);

public sealed record UsuarioUpdateDto(
    string NombreCompleto,
    int IdRol,
    int? IdUbicacion,
    bool Activo);

public sealed record CambiarContrasenaDto(
    string ContrasenaActual,
    string NuevaContrasena);