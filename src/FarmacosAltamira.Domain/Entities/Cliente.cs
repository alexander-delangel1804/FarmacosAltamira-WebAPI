namespace FarmacosAltamira.Domain.Entities;

public sealed class Cliente
{
    public int IdCliente { get; set; }
    public string Rfc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RegimenFiscal { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string UsoCfdi { get; set; } = string.Empty;
    public string? Correo { get; set; }
}