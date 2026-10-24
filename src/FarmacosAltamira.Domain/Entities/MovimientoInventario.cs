using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class MovimientoInventario
{
    public long IdMovimiento { get; set; }
    public int IdLote { get; set; }
    public int IdUbicacion { get; set; }
    public int IdUsuario { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public TipoMovimientoInventario Tipo { get; set; }
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public string? DocumentoOrigen { get; set; }
    public long? IdDocumento { get; set; }

    public Lote Lote { get; set; } = null!;
    public Ubicacion Ubicacion { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}