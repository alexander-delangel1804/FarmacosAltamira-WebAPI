namespace FarmacosAltamira.Domain.Entities;

public sealed class ProveedorPagoServicio
{
    public int IdProveedorPago { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<CatalogoServicio> Servicios { get; set; } = new List<CatalogoServicio>();
}