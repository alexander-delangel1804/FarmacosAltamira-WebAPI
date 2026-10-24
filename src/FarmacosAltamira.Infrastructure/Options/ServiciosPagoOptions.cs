namespace FarmacosAltamira.Infrastructure.Options;

public sealed class ServiciosPagoOptions
{
    public const string Seccion = "ServiciosPago";
    public string Modo { get; set; } = "Fake"; // Fake, Sandbox, Produccion
    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int TimeoutSegundos { get; set; } = 30;
}