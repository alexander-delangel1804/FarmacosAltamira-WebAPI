using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class CorteCaja
{
    public int IdCorte { get; set; }
    public int IdCaja { get; set; }
    public int IdUsuario { get; set; }
    public DateTimeOffset FechaApertura { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public decimal FondoInicial { get; set; }
    public decimal? TotalSistema { get; set; }
    public decimal? TotalContado { get; set; }
    public decimal? Diferencia { get; set; }
    public EstadoCorte Estado { get; set; } = EstadoCorte.Abierto;
    public Guid? UuidOperacion { get; set; }

    public Caja Caja { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<PagoServicio> PagosServicio { get; set; } = new List<PagoServicio>();
}