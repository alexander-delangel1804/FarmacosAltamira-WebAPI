namespace FarmacosAltamira.Domain.Entities;

public sealed class ParametroSistema
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTimeOffset FechaModificacion { get; set; }
}