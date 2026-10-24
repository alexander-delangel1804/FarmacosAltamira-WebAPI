using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using FarmacosAltamira.Domain.Enums;
using FarmacosAltamira.Domain.ValueObjects;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmacosAltamira.Worker.Services;

public sealed class TimbradoWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TimbradoWorker> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(5);

    public TimbradoWorker(IServiceProvider services, ILogger<TimbradoWorker> logger)
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
                var pac = scope.ServiceProvider.GetRequiredService<IPacProvider>();

                // Tomar bloqueo asesor
                var lockResult = await db.Database.SqlQueryRaw<int>("SELECT pg_try_advisory_lock(123456790)").FirstAsync(stoppingToken);
                if (lockResult != 1)
                {
                    await Task.Delay(_intervalo, stoppingToken);
                    continue;
                }

                try
                {
                    var pendientes = await db.ComprobantesFiscales
                        .Where(c => (c.Estatus == EstatusComprobanteFiscal.Pendiente || c.Estatus == EstatusComprobanteFiscal.Error)
                            && (!c.ProximoReintento.HasValue || c.ProximoReintento <= DateTimeOffset.UtcNow))
                        .OrderBy(c => c.ProximoReintento ?? DateTimeOffset.MaxValue)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    foreach (var comprobante in pendientes)
                    {
                        try
                        {
                            _logger.LogInformation("Reintentando timbrado para comprobante {Id}", comprobante.IdComprobante);

                            var venta = await db.Ventas
                                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                                .Include(v => v.Pagos)
                                .Include(v => v.Corte).ThenInclude(c => c.Caja).ThenInclude(c => c.Ubicacion)
                                .Include(v => v.Usuario)
                                .FirstOrDefaultAsync(v => v.IdVenta == comprobante.IdVenta, stoppingToken);

                            if (venta == null) continue;

                            var solicitud = ConstruirSolicitudTimbrado(venta, comprobante);
                            var resultado = await pac.TimbrarAsync(solicitud, stoppingToken);

                            await RegistrarEventoAsync(db, comprobante.IdComprobante, "REINTENTO", resultado, stoppingToken);

                            if (resultado.Estado == EstadoResultadoPac.Timbrado)
                            {
                                comprobante.Estatus = EstatusComprobanteFiscal.Timbrado;
                                comprobante.UuidCfdi = resultado.UuidCfdi;
                                comprobante.Xml = resultado.Xml;
                                comprobante.FechaTimbrado = DateTimeOffset.UtcNow;
                                comprobante.XmlHash = resultado.Xml != null ? Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(resultado.Xml))) : null;

                                venta.Estado = EstadoVenta.Facturada;
                                await db.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation("Timbrado exitoso para comprobante {Id}", comprobante.IdComprobante);
                            }
                            else if (resultado.Estado == EstadoResultadoPac.Rechazado)
                            {
                                comprobante.Estatus = EstatusComprobanteFiscal.Error;
                                comprobante.UltimoError = resultado.Mensaje;
                                await db.SaveChangesAsync(stoppingToken);
                            }
                            else if (resultado.Estado == EstadoResultadoPac.NoDisponible || resultado.Estado == EstadoResultadoPac.Indeterminado)
                            {
                                comprobante.Reintentos++;
                                var maxReintentos = 10; // De parámetro
                                if (comprobante.Reintentos >= maxReintentos)
                                {
                                    comprobante.Estatus = EstatusComprobanteFiscal.Error;
                                    comprobante.UltimoError = "Máximo de reintentos alcanzado";
                                }
                                else
                                {
                                    var backoff = TimeSpan.FromMinutes(Math.Pow(2, comprobante.Reintentos) * 5);
                                    comprobante.ProximoReintento = DateTimeOffset.UtcNow.Add(backoff);
                                }
                                await db.SaveChangesAsync(stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error procesando comprobante {Id}", comprobante.IdComprobante);
                        }
                    }
                }
                finally
                {
                    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(123456790)", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en TimbradoWorker");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private static SolicitudTimbrado ConstruirSolicitudTimbrado(Venta venta, ComprobanteFiscal comprobante)
    {
        var ubicacion = venta.Corte.Caja.Ubicacion;
        var emisor = ubicacion.IdEmisor != null ? "TODO: obtener emisor" : "EMISOR_DEFAULT";

        return new SolicitudTimbrado(
            venta.IdVenta,
            emisor,
            "Razón Social",
            "601", // Régimen general
            ubicacion.Municipio ?? "00000",
            comprobante.Serie ?? "A",
            comprobante.Folio ?? venta.Folio,
            venta.Comprobante?.IdCliente != null ? "RFC_CLIENTE" : "XAXX010101000",
            venta.Comprobante?.IdCliente != null ? "Cliente" : "Público en general",
            "601",
            "00000",
            "G03",
            venta.Detalles.Select(d => new ConceptoTimbrado(
                d.Producto.DatosFiscales?.ClaveProdServ ?? "01010101",
                d.Producto.DatosFiscales?.ClaveUnidad ?? "H87",
                d.Producto.NombreComercial,
                d.Cantidad,
                d.PrecioUnitario,
                d.Importe,
                d.Producto.DatosFiscales?.ObjetoImp ?? "02",
                d.Producto.TasaIva,
                d.Producto.DatosFiscales?.IepsTasa)).ToList(),
            venta.Pagos.Select(p => new PagoTimbrado(p.FormaPago.ToString(), p.Monto)).ToList(),
            "PUE",
            venta.Pagos.FirstOrDefault()?.FormaPago.ToString() ?? "01");
    }

    private static async Task RegistrarEventoAsync(CentralDbContext db, int idComprobante, string tipo, ResultadoPac resultado, CancellationToken ct)
    {
        var evento = new ComprobanteFiscalEvento
        {
            IdComprobante = idComprobante,
            Fecha = DateTimeOffset.UtcNow,
            Tipo = tipo,
            EstadoPac = resultado.Estado.ToString(),
            CodigoPac = resultado.CodigoPac,
            Mensaje = resultado.Mensaje,
            PayloadHash = resultado.Xml != null ? Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(resultado.Xml))) : null
        };

        db.ComprobanteFiscalEventos.Add(evento);
        await db.SaveChangesAsync(ct);
    }
}