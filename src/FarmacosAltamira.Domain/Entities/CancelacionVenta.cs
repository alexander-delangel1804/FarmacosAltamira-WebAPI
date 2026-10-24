namespace FarmacosAltamira.Domain.Entities;

public sealed class CancelacionVenta
{
    public int IdCancelacion { get; set; }
    public int IdVenta { get; set; }
    public int IdUsuarioSolicita { get; set; }
    public int IdUsuarioAutoriza { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public bool CfdiCancelado { get; set; } = false;

    public Venta Venta { get; set; } = null!;
    public Usuario UsuarioSolicita { get; set; } = null!;
    public Usuario UsuarioAutoriza { get; set; } = null!;
}