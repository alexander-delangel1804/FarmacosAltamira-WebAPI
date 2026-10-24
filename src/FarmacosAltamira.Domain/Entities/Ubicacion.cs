using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class Ubicacion
{
    public int IdUbicacion { get; set; }
    public TipoUbicacion Tipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Municipio { get; set; }
    public string? Telefono { get; set; }
    public string? CapacidadReferencia { get; set; }
    public bool Activa { get; set; } = true;
    public int? IdEmisor { get; set; }
    public EmisorFiscal? EmisorFiscal { get; set; }
    public string? SerieCfdi { get; set; }
}