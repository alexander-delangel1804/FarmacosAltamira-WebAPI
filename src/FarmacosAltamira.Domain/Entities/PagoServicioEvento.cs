namespace FarmacosAltamira.Domain.Entities;

public sealed class PagoServicioEvento
{
    public long IdEvento { get; set; }
    public int IdPagoServicio { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Operacion { get; set; } = string.Empty;
    public string? EstadoProveedor { get; set; }
    public string? CodigoProveedor { get; set; }
    public string? Mensaje { get; set; }

    public PagoServicio PagoServicio { get; set; } = null!;
}