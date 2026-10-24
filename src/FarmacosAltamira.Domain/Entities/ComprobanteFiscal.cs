using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class ComprobanteFiscal
{
    public int IdComprobante { get; set; }
    public int IdVenta { get; set; }
    public int? IdCliente { get; set; }
    public int? IdEmisor { get; set; }
    public Guid? UuidCfdi { get; set; }
    public string? Serie { get; set; }
    public string? Folio { get; set; }
    public DateTimeOffset? FechaTimbrado { get; set; }
    public EstatusComprobanteFiscal Estatus { get; set; } = EstatusComprobanteFiscal.Pendiente;
    public string? Xml { get; set; }
    public int Reintentos { get; set; }
    public string? UltimoError { get; set; }
    public DateTimeOffset? ProximoReintento { get; set; }
    public string? XmlHash { get; set; }

    public Venta Venta { get; set; } = null!;
    public Cliente? Cliente { get; set; }
    public EmisorFiscal? Emisor { get; set; }
    public ICollection<ComprobanteFiscalEvento> Eventos { get; set; } = new List<ComprobanteFiscalEvento>();
}