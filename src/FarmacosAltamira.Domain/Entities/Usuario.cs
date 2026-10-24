namespace FarmacosAltamira.Domain.Entities;

public sealed class Usuario
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public int? IdUbicacion { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public bool Bloqueado { get; set; } = false;
    public int IntentosFallidos { get; set; } = 0;
    public DateTimeOffset? UltimoAcceso { get; set; }
    public DateOnly FechaAlta { get; set; }

    public Rol Rol { get; set; } = null!;
    public Ubicacion? Ubicacion { get; set; }
}