using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Application.Caja.Services;
using FarmacosAltamira.Application.Catalogo.Services;
using FarmacosAltamira.Application.Inventario.Services;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Application.Traslados.Services;
using FarmacosAltamira.Application.Ventas.Services;
using FarmacosAltamira.Infrastructure.HealthChecks;
using FarmacosAltamira.Infrastructure.Integraciones;
using FarmacosAltamira.Infrastructure.Integraciones.Pac;
using FarmacosAltamira.Infrastructure.Integraciones.Servicios;
using FarmacosAltamira.Infrastructure.Options;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FarmacosAltamira.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var central = NormalizarCadenaConexion(
            configuration.GetConnectionString("Central")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Central'."));

        var reportes = NormalizarCadenaConexion(configuration.GetConnectionString("Reportes")) ?? central;

        // IMPORTANTE: sin EnableRetryOnFailure. La estrategia de reintentos de EF no permite las
        // transacciones manuales que usan ventas, traslados y cortes.
        services.AddDbContext<CentralDbContext>(options =>
        {
            options.UseSnakeCaseNamingConvention();
            options.UseNpgsql(central, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(30); // 30 seconds timeout for commands
            });
        });

        // Register ICentralDbContext for Application layer
        services.AddScoped<ICentralDbContext>(sp => sp.GetRequiredService<CentralDbContext>());

        services.AddDbContext<ReportesDbContext>(options =>
        {
            options.UseSnakeCaseNamingConvention();
            options.UseNpgsql(reportes, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(30);
            })
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // LocalDbContext for SQLite (POS local database)
        var localConnStr = configuration.GetConnectionString("Local") 
            ?? "Data Source=local.db;Cache=Shared;Foreign Keys=True";
        
        services.AddDbContext<LocalDbContext>(options =>
        {
            options.UseSqlite(localConnStr);
        });

        // Register ILocalDbContext for Application layer
        services.AddScoped<ILocalDbContext>(sp => sp.GetRequiredService<LocalDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Seguridad
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditoria, AuditoriaService>();
        services.AddScoped<IEstacionService, EstacionService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<RolService>();
        services.AddScoped<BitacoraService>();
        services.AddScoped<ParametroService>();
        services.AddScoped<RespaldoService>();
        services.AddScoped<AuthService>();

        // Catálogo, inventario, traslados, caja y ventas
        services.AddScoped<IKardexService, KardexService>();
        services.AddScoped<IExistenciaService, ExistenciaService>();
        services.AddScoped<ProductoService>();
        services.AddScoped<CategoriaService>();
        services.AddScoped<LoteService>();
        services.AddScoped<TrasladoService>();
        services.AddScoped<CorteService>();
        services.AddScoped<VentaService>();

        // Integraciones externas: PAC y pago de servicios (diseño aún en definición)
        services.Configure<JwtTokenOptions>(configuration.GetSection("Jwt"));
        services.Configure<PacOptions>(configuration.GetSection(PacOptions.Seccion));
        services.Configure<ServiciosPagoOptions>(configuration.GetSection(ServiciosPagoOptions.Seccion));
        RegistrarPac(services, configuration);
        RegistrarServiciosPago(services, configuration);

        // Health checks: "live" no depende de la BD; "ready" verifica PostgreSQL con timeout
        var centralConnStr = configuration.GetConnectionString("Central") ?? string.Empty;
        
        services.AddHealthChecks()
            .AddNpgSql(
                centralConnStr,
                name: "postgres",
                tags: new[] { "ready" },
                timeout: TimeSpan.FromSeconds(10))
            .AddCheck<DatabaseReadyHealthCheck>("database-ready", tags: new[] { "ready" });

        services.Configure<DatabaseHealthCheckOptions>(options =>
        {
            options.ConnectionString = centralConnStr;
        });

        return services;
    }

    private static void RegistrarPac(IServiceCollection services, IConfiguration configuration)
    {
        var modo = configuration[$"{PacOptions.Seccion}:Modo"] ?? "Fake";
        switch (modo)
        {
            case "Fake":
                services.AddSingleton<IPacProvider, FakePacProvider>();
                break;
            // Cuando se defina el PAC: case "Sandbox": case "Produccion": registrar aquí su adaptador
            // (clase que implemente IPacProvider) con AddHttpClient + resiliencia.
            default:
                throw new InvalidOperationException(
                    $"Pac:Modo='{modo}' aún no tiene adaptador. Use 'Fake' o registre su IPacProvider en DependencyInjection.RegistrarPac.");
        }
    }

    private static void RegistrarServiciosPago(IServiceCollection services, IConfiguration configuration)
    {
        var modo = configuration[$"{ServiciosPagoOptions.Seccion}:Modo"] ?? "Fake";
        switch (modo)
        {
            case "Fake":
                services.AddSingleton<IProveedorServicios, FakeProveedorServicios>();
                break;
            default:
                throw new InvalidOperationException(
                    $"ServiciosPago:Modo='{modo}' aún no tiene adaptador. Use 'Fake' o registre su IProveedorServicios en DependencyInjection.RegistrarServiciosPago.");
        }
    }

    /// <summary>
    /// Acepta tanto el formato de Npgsql como la URL de Render
    /// (postgresql://usuario:clave@host[:puerto]/base) y la convierte a formato Npgsql.
    /// </summary>
    internal static string? NormalizarCadenaConexion(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return valor;
        if (!valor.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !valor.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return valor;

        var uri = new Uri(valor);
        var credenciales = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credenciales[0]),
            Password = credenciales.Length > 1 ? Uri.UnescapeDataString(credenciales[1]) : string.Empty,
            MaxPoolSize = 20
        };

        // El host externo de Render exige TLS; el interno (red privada) no.
        if (uri.Host.Contains('.'))
        {
            builder.SslMode = SslMode.Require;
            builder.TrustServerCertificate = true;
        }

        return builder.ConnectionString;
    }
}
