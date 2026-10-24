namespace FarmacosAltamira.Domain.Entities;

public sealed class Estacion
{
    public int IdEstacion { get; set; }
    public int IdUbicacion { get; set; }
    public int? IdCaja { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string HashSecreto { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public string? VersionCliente { get; set; }
    public DateTimeOffset? UltimaConexion { get; set; }

    public Ubicacion Ubicacion { get; set; } = null!;
    public Caja? Caja { get; set; }
}