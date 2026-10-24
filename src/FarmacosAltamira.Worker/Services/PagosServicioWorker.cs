using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Domain.ValueObjects;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmacosAltamira.Worker.Services;

public sealed class PagosServicioWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PagosServicioWorker> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(5);

    public PagosServicioWorker(IServiceProvider services, ILogger<PagosServicioWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
                var proveedor = scope.ServiceProvider.GetRequiredService<IProveedorServicios>();

                // Bloqueo asesor
                var lockResult = await db.Database.SqlQueryRaw<int>("SELECT pg_try_advisory_lock(123456791)").FirstAsync(stoppingToken);
                if (lockResult != 1)
                {
                    await Task.Delay(_intervalo, stoppingToken);
                    continue;
                }

                try
                {
                    var pendientes = await db.PagosServicio
                        .Where(p => p.Estatus == EstatusPagoServicio.EnVerificacion
                            && (!p.FechaRespuesta.HasValue || p.FechaRespuesta <= DateTimeOffset.UtcNow))
                        .OrderBy(p => p.FechaHora)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    foreach (var pago in pendientes)
                    {
                        try
                        {
                            _logger.LogInformation("Verificando pago de servicio {Id}", pago.IdPagoServicio);

                            var resultado = await proveedor.ConsultarEstatusAsync(pago.FolioAutorizacion ?? pago.Folio, stoppingToken);

                            await RegistrarEventoAsync(db, pago.IdPagoServicio, "ESTATUS", resultado, stoppingToken);

                            if (resultado.Estado == EstadoResultadoServicio.Autorizado)
                            {
                                pago.Estatus = EstatusPagoServicio.Autorizado;
                                pago.FechaRespuesta = DateTimeOffset.UtcNow;
                                pago.CodigoRespuesta = resultado.CodigoProveedor;
                                await db.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation("Pago de servicio {Id} autorizado", pago.IdPagoServicio);
                            }
                            else if (resultado.Estado == EstadoResultadoServicio.Rechazado)
                            {
                                pago.Estatus = EstatusPagoServicio.Rechazado;
                                pago.FechaRespuesta = DateTimeOffset.UtcNow;
                                pago.CodigoRespuesta = resultado.CodigoProveedor;
                                await db.SaveChangesAsync(stoppingToken);
                            }
                            else
                            {
                                pago.Intentos++;
                                if (pago.Intentos >= 20)
                                {
                                    pago.Estatus = EstatusPagoServicio.Rechazado;
                                    pago.CodigoRespuesta = "MAX_REINTENTOS";
                                }
                                else
                                {
                                    var backoff = TimeSpan.FromMinutes(Math.Pow(2, pago.Intentos) * 2);
                                    pago.FechaRespuesta = DateTimeOffset.UtcNow.Add(backoff);
                                }
                                await db.SaveChangesAsync(stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error verificando pago {Id}", pago.IdPagoServicio);
                        }
                    }
                }
                finally
                {
                    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(123456791)", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en PagosServicioWorker");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private static async Task RegistrarEventoAsync(CentralDbContext db, int idPagoServicio, string operacion, ResultadoServicio resultado, CancellationToken ct)
    {
        var evento = new PagoServicioEvento
        {
            IdPagoServicio = idPagoServicio,
            Fecha = DateTimeOffset.UtcNow,
            Operacion = operacion,
            EstadoProveedor = resultado.Estado.ToString(),
            CodigoProveedor = resultado.CodigoProveedor,
            Mensaje = resultado.Mensaje
        };

        db.PagosServicioEventos.Add(evento);
        await db.SaveChangesAsync(ct);
    }
}