namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class ColaSincronizacion
{
    public int IdCola { get; set; }
    public string UuidOperacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // VENTA, DEVOLUCION, CANCELACION, CORTE
    public int IdReferencia { get; set; } // id_local del registro origen
    public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, ENVIANDO, HECHA, ERROR_FATAL
    public int Intentos { get; set; }
    public string? ProximoIntento { get; set; } // ISO8601
    public string? UltimoError { get; set; }
    public string CreadoEn { get; set; } = string.Empty; // ISO8601
}