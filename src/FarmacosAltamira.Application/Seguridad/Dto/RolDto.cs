namespace FarmacosAltamira.Application.Seguridad.Dto;

public sealed record RolDto(
    int IdRol,
    string Nombre,
    string? Descripcion,
    IReadOnlyList<PermisoDto> Permisos);

public sealed record PermisoDto(
    int IdPermiso,
    string Modulo,
    string Accion);

public sealed record RolCreateDto(
    string Nombre,
    string? Descripcion,
    IReadOnlyList<int> PermisosIds);

public sealed record RolUpdateDto(
    string? Descripcion,
    IReadOnlyList<int> PermisosIds);