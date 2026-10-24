namespace FarmacosAltamira.Domain.Entities;

public sealed class ComprobanteFiscalEvento
{
    public long IdEvento { get; set; }
    public int IdComprobante { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? EstadoPac { get; set; }
    public int? HttpStatus { get; set; }
    public string? CodigoPac { get; set; }
    public string? Mensaje { get; set; }
    public string? PayloadHash { get; set; }

    public ComprobanteFiscal Comprobante { get; set; } = null!;
}