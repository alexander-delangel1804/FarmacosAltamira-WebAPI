namespace FarmacosAltamira.Domain.Entities;

public sealed class Rol
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}