namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class RecetaLocal
{
    public int IdRecetaLocal { get; set; }
    public string FolioReceta { get; set; } = string.Empty;
    public string TipoReceta { get; set; } = string.Empty;
    public string FechaExpedicion { get; set; } = string.Empty; // ISO8601
    public string NombreMedico { get; set; } = string.Empty;
    public string CedulaMedico { get; set; } = string.Empty;
    public string? Institucion { get; set; }
    public bool Retenida { get; set; }
}