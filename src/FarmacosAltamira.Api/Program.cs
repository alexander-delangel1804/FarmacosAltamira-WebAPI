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

app.Run();