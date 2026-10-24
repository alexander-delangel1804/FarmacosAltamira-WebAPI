namespace FarmacosAltamira.Domain.Entities;

public sealed class SesionUsuario
{
    public int IdSesion { get; set; }
    public int IdUsuario { get; set; }
    public string HashToken { get; set; } = string.Empty;
    public DateTimeOffset Expiracion { get; set; }
    public bool Revocado { get; set; } = false;

    public Usuario Usuario { get; set; } = null!;
}