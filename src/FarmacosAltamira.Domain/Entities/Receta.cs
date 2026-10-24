using FarmacosAltamira.Domain.Enums;

namespace FarmacosAltamira.Domain.Entities;

public sealed class Receta
{
    public int IdReceta { get; set; }
    public string FolioReceta { get; set; } = string.Empty;
    public TipoReceta TipoReceta { get; set; }
    public DateOnly FechaExpedicion { get; set; }
    public string NombreMedico { get; set; } = string.Empty;
    public string CedulaMedico { get; set; } = string.Empty;
    public string? Institucion { get; set; }
    public bool Retenida { get; set; } = false;

    public ICollection<RegistroControlado> RegistrosControlados { get; set; } = new List<RegistroControlado>();
}