using FarmacosAltamira.Infrastructure;
using FarmacosAltamira.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddSerilog((services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/worker-.txt", rollingInterval: RollingInterval.Day);
});

builder.Services.AddInfrastructure(configuration);
// builder.Services.AddHostedService<AlertasWorker>();          // Comentado: no crítico para arranque
// builder.Services.AddHostedService<TimbradoWorker>();         // Comentado: no crítico para arranque
// builder.Services.AddHostedService<PagosServicioWorker>();    // Comentado: no crítico para arranque
// builder.Services.AddHostedService<RespaldoWorker>();         // Comentado: no crítico para arranque

var host = builder.Build();
host.Run();