namespace FarmacosAltamira.Infrastructure.Options;

public sealed class PacOptions
{
    public const string Seccion = "Pac";
    public string Modo { get; set; } = "Fake"; // Fake, Sandbox, Produccion
    public string Url { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int TimeoutSegundos { get; set; } = 30;
}