namespace FarmacosAltamira.Domain.Entities;

public sealed class EmisorFiscal
{
    public int IdEmisor { get; set; }
    public string Rfc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RegimenFiscal { get; set; } = string.Empty;
    public string CpExpedicion { get; set; } = string.Empty;
    public string? RefCertificado { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();
    public ICollection<ComprobanteFiscal> Comprobantes { get; set; } = new List<ComprobanteFiscal>();
}