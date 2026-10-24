namespace FarmacosAltamira.Domain.Entities;

public sealed class Caja
{
    public int IdCaja { get; set; }
    public int IdUbicacion { get; set; }
    public int Numero { get; set; }
    public bool Activa { get; set; } = true;

    public Ubicacion Ubicacion { get; set; } = null!;
    public ICollection<CorteCaja> Cortes { get; set; } = new List<CorteCaja>();
}