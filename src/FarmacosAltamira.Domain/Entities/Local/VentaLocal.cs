namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class VentaLocal
{
    public int IdLocal { get; set; }
    public string UuidOperacion { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public int IdCorteLocal { get; set; }
    public int IdUsuario { get; set; }
    public string FechaHora { get; set; } = string.Empty; // ISO8601
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "PAGADA"; // EN_CAPTURA, PAGADA, FACTURACION_PENDIENTE, etc.
    public bool RequiereFactura { get; set; }
    public string SyncEstado { get; set; } = "PENDIENTE"; // PENDIENTE, ENVIANDO, SINCRONIZADA
    public int SyncIntentos { get; set; }
    public string? SyncUltimoError { get; set; }
    public string? SyncFecha { get; set; } // ISO8601
    public int? IdVentaCentral { get; set; }
}