namespace FarmacosAltamira.Domain.Entities;

public sealed class Permiso
{
    public int IdPermiso { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public ICollection<Rol> Roles { get; set; } = new List<Rol>();
}