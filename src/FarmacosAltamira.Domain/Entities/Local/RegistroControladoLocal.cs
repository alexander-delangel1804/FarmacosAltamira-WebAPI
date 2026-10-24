namespace FarmacosAltamira.Domain.Entities.Local;

public sealed class RegistroControladoLocal
{
    public int IdRegistroLocal { get; set; }
    public int IdRecetaLocal { get; set; }
    public int IdDetalleLocal { get; set; }
    public int IdProducto { get; set; }
    public int IdLote { get; set; }
    public int IdUsuario { get; set; }
    public string FechaHora { get; set; } = string.Empty; // ISO8601
    public int CantidadSurtida { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
    public int ExistenciaPosterior { get; set; }
}