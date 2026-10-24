using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FarmacosAltamira.Worker.Services;

public sealed class AlertasWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AlertasWorker> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromHours(24);

    public AlertasWorker(IServiceProvider services, ILogger<AlertasWorker> logger)
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

                // Tomar bloqueo asesor para evitar ejecución doble
                var lockResult = await db.Database.SqlQueryRaw<int>("SELECT pg_try_advisory_lock(123456789)").FirstAsync(stoppingToken);
                if (lockResult != 1)
                {
                    _logger.LogInformation("Otra instancia está ejecutando alertas, omitiendo");
                    await Task.Delay(_intervalo, stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation("Ejecutando generación de alertas de caducidad y stock mínimo");

                    await db.Database.ExecuteSqlRawAsync("SELECT sp_generar_alertas_caducidad()", stoppingToken);
                    await db.Database.ExecuteSqlRawAsync("SELECT sp_generar_alertas_stock_minimo()", stoppingToken);
                    await db.Database.ExecuteSqlRawAsync("SELECT sp_recalcular_estado_lotes()", stoppingToken);

                    _logger.LogInformation("Alertas generadas exitosamente");
                }
                finally
                {
                    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(123456789)", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AlertasWorker");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }
}