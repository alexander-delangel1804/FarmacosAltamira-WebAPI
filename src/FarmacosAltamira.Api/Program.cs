using FarmacosAltamira.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseApiPipeline();

// Ensure database is created (migrations are handled by SQL scripts)
// This is just for development - in production, the DB is managed externally
// COMENTADO PARA EVITAR BLOQUEOS AL INICIO
/*
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FarmacosAltamira.Infrastructure.Persistence.CentralDbContext>();
    await db.Database.EnsureCreatedAsync();
}
*/

app.Run();