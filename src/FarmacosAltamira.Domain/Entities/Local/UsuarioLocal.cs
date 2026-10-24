namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class UsuarioLocal
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public bool Bloqueado { get; set; }
    public string ActualizadoEn { get; set; } = string.Empty; // ISO8601
}