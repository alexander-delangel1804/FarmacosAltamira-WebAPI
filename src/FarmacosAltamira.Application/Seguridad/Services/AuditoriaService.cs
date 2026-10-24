using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class AuditoriaService : IAuditoria
{
    private readonly ICentralDbContext _db;

    public AuditoriaService(ICentralDbContext db)
    {
        _db = db;
    }

    public async Task RegistrarAsync(
        int idUsuario,
        int? idUbicacion,
        string modulo,
        string accion,
        string? entidad = null,
        string? idEntidad = null,
        string? detalle = null,
        string? ipOrigen = null,
        CancellationToken cancellationToken = default)
    {
        var evento = new BitacoraEvento
        {
            IdUsuario = idUsuario,
            IdUbicacion = idUbicacion,
            FechaHora = DateTimeOffset.UtcNow,
            Modulo = modulo,
            Accion = accion,
            Entidad = entidad,
            IdEntidad = idEntidad,
            Detalle = detalle,
            IpOrigen = ipOrigen
        };

        _db.BitacoraEventos.Add(evento);
        await _db.SaveChangesAsync(cancellationToken);
    }
}