using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class Promocion
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoPromocion Tipo { get; set; }
    public decimal Valor { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public bool Activa { get; set; } = true;
}