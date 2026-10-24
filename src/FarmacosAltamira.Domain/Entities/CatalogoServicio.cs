using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class CatalogoServicio
{
    public int IdServicio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public TipoComision TipoComision { get; set; }
    public decimal ValorComision { get; set; }
    public bool Activo { get; set; } = true;
    public int? IdProveedorPago { get; set; }
    public string? CodigoProveedor { get; set; }
    public string? EtiquetaReferencia { get; set; }
    public string? PatronReferencia { get; set; }
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }

    public ProveedorPagoServicio? Proveedor { get; set; }
    public ICollection<PagoServicio> Pagos { get; set; } = new List<PagoServicio>();
}