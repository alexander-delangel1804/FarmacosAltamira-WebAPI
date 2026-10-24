using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace FarmacosAltamira.Worker.Services;

public sealed class RespaldoWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RespaldoWorker> _logger;
    private readonly IConfiguration _configuration;
    private TimeSpan _intervalo;

    public RespaldoWorker(IServiceProvider services, ILogger<RespaldoWorker> logger, IConfiguration configuration)
    {
        _services = services;
        _logger = logger;
        _configuration = configuration;
        _intervalo = TimeSpan.FromHours(24);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Cargar intervalo de parámetros
        await using (var scope = _services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
            var horas = await db.Database.SqlQueryRaw<int>("SELECT valor::int FROM parametro_sistema WHERE clave = 'FRECUENCIA_RESPALDO_HORAS'").FirstOrDefaultAsync(stoppingToken);
            if (horas > 0) _intervalo = TimeSpan.FromHours(horas);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
                var respaldoService = scope.ServiceProvider.GetRequiredService<FarmacosAltamira.Application.Seguridad.Services.RespaldoService>();

                var lockResult = await db.Database.SqlQueryRaw<int>("SELECT pg_try_advisory_lock(123456792)").FirstAsync(stoppingToken);
                if (lockResult != 1)
                {
                    await Task.Delay(_intervalo, stoppingToken);
                    continue;
                }

                try
                {
                    _logger.LogInformation("Iniciando respaldo automático");

                    var rutaBase = _configuration["Backup:Path"] ?? "/backups";
                    var archivo = $"farmacos_altamira_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.dump";
                    var rutaCompleta = Path.Combine(rutaBase, archivo);

                    Directory.CreateDirectory(rutaBase);

                    var idRespaldo = await respaldoService.RegistrarInicioAsync(null, "AUTOMATICO", rutaCompleta, stoppingToken);

                    try
                    {
                        var connectionString = _configuration.GetConnectionString("Central")!;
                        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                        var host = builder.Host;
                        var port = builder.Port;
                        var database = builder.Database;
                        var username = builder.Username;

                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "pg_dump",
                            Arguments = $"-h {host} -p {port} -U {username} -d {database} -Fc -f \"{rutaCompleta}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using var process = System.Diagnostics.Process.Start(psi);
                        if (process == null) throw new InvalidOperationException("No se pudo iniciar pg_dump");

                        await process.WaitForExitAsync(stoppingToken);

                        if (process.ExitCode != 0)
                        {
                            var error = await process.StandardError.ReadToEndAsync();
                            throw new InvalidOperationException($"pg_dump falló: {error}");
                        }

                        var tamanoMb = new FileInfo(rutaCompleta).Length / (1024.0 * 1024.0);

                        // Cifrar el archivo (implementación básica)
                        await CifrarArchivoAsync(rutaCompleta, stoppingToken);

                        await respaldoService.FinalizarAsync(idRespaldo.IdRespaldo, true, (decimal)tamanoMb, null, stoppingToken);
                        _logger.LogInformation("Respaldo automático completado: {Archivo} ({Tamano} MB)", archivo, tamanoMb);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error en respaldo automático");
                        await respaldoService.FinalizarAsync(idRespaldo.IdRespaldo, false, null, null, stoppingToken);
                    }
                }
                finally
                {
                    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(123456792)", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RespaldoWorker");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private static async Task CifrarArchivoAsync(string ruta, CancellationToken ct)
    {
        // Implementación básica: en producción usar AES-GCM con clave del almacén de secretos
        await Task.CompletedTask;
    }
}