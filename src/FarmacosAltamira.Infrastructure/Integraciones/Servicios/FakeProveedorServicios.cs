using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Domain.ValueObjects;

namespace FarmacosAltamira.Infrastructure.Integraciones.Servicios;

public sealed class FakeProveedorServicios : IProveedorServicios
{
    private readonly Dictionary<string, ResultadoServicio> _pagos = new();
    private int _callCount = 0;

    public Task<ResultadoServicio> ConsultarAdeudoAsync(ConsultaAdeudo consulta, CancellationToken ct)
    {
        _callCount++;
        return Task.FromResult(new ResultadoServicio(
            EstadoResultadoServicio.Autorizado,
            "ADEUDO-12345",
            "00",
            $"Adeudo consultado: ${150.00:F2} (SIMULADO)"));
    }

    public Task<ResultadoServicio> PagarAsync(SolicitudPagoServicio solicitud, CancellationToken ct)
    {
        _callCount++;
        var folio = $"FOL-{DateTimeOffset.UtcNow:yyyyMMdd}-{_callCount:D6}";

        var resultado = new ResultadoServicio(
            EstadoResultadoServicio.Autorizado,
            folio,
            "00",
            "Pago autorizado (SIMULADO)");

        _pagos[folio] = resultado;
        return Task.FromResult(resultado);
    }

    public Task<ResultadoServicio> ConsultarEstatusAsync(string referenciaOperacion, CancellationToken ct)
    {
        if (_pagos.TryGetValue(referenciaOperacion, out var resultado))
        {
            return Task.FromResult(resultado);
        }

        return Task.FromResult(new ResultadoServicio(
            EstadoResultadoServicio.Rechazado,
            null,
            "NO_ENCONTRADO",
            "Operación no encontrada"));
    }

    public Task<ResultadoServicio> ReversarAsync(SolicitudReverso solicitud, CancellationToken ct)
    {
        if (_pagos.TryGetValue(solicitud.IdPagoServicio.ToString(), out var resultado))
        {
            var reversado = resultado with
            {
                Estado = EstadoResultadoServicio.Reversado,
                Mensaje = "Pago reversado (SIMULADO)"
            };
            _pagos[solicitud.IdPagoServicio.ToString()] = reversado;
            return Task.FromResult(reversado);
        }

        return Task.FromResult(new ResultadoServicio(
            EstadoResultadoServicio.Rechazado,
            null,
            "NO_ENCONTRADO",
            "Pago no encontrado para reversar"));
    }

    public void ConfigurarFallo()
    {
        // Para pruebas
    }

    public void ConfigurarNoDisponible()
    {
        // Para pruebas
    }

    public void ConfigurarIndeterminado()
    {
        // Para pruebas
    }
}