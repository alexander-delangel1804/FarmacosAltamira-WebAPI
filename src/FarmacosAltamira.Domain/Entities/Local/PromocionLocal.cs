namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class PromocionLocal
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // PORCENTAJE, MONTO
    public decimal Valor { get; set; }
    public string FechaInicio { get; set; } = string.Empty; // ISO8601
    public string FechaFin { get; set; } = string.Empty; // ISO8601
    public bool Activa { get; set; }
}