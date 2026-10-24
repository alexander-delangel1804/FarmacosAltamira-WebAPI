namespace FarmacosAltamira.Domain.Entities;

public sealed class Respaldo
{
    public int IdRespaldo { get; set; }
    public int? IdUsuario { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public decimal? TamanoMb { get; set; }
    public bool Cifrado { get; set; } = true;
    public string Estado { get; set; } = string.Empty;
    public bool Verificado { get; set; } = false;

    public Usuario? Usuario { get; set; }
}