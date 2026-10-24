using FarmacosAltamira.Domain.ValueObjects;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IProveedorServicios
{
    Task<ResultadoServicio> ConsultarAdeudoAsync(ConsultaAdeudo consulta, CancellationToken ct);
    Task<ResultadoServicio> PagarAsync(SolicitudPagoServicio solicitud, CancellationToken ct);
    Task<ResultadoServicio> ConsultarEstatusAsync(string referenciaOperacion, CancellationToken ct);
    Task<ResultadoServicio> ReversarAsync(SolicitudReverso solicitud, CancellationToken ct);
}