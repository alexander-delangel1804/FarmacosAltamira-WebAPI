namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class CorteCajaLocal
{
    public int IdCorteLocal { get; set; }
    public int? IdCorteCentral { get; set; }
    public int IdUsuario { get; set; }
    public string FechaApertura { get; set; } = string.Empty; // ISO8601
    public string? FechaCierre { get; set; } // ISO8601
    public decimal FondoInicial { get; set; }
    public decimal? TotalSistema { get; set; }
    public decimal? TotalContado { get; set; }
    public decimal? Diferencia { get; set; }
    public string Estado { get; set; } = "ABIERTO"; // ABIERTO, CERRADO
}