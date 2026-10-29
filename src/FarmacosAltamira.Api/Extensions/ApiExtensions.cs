using FarmacosAltamira.Api.Middleware;
using FarmacosAltamira.Application.Seguridad.Services;
using FarmacosAltamira.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace FarmacosAltamira.Api.Extensions;

public sealed class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errores = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

            var detalles = new
            {
                errores,
                cantidad = context.ModelState.ErrorCount
            };

            var error = new FarmacosAltamira.Api.Middleware.ProblemDetailsResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Bad Request",
                Detail = "Error de validación en los datos de entrada",
                Codigo = "SYNC_VALIDATION_ERROR",
                Detalles = detalles
            };

            context.Result = new ObjectResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Serilog
        services.AddSerilog((sp, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day);
        });

        // Controllers
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidateModelFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        // Authentication
        var jwtSecret = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "rol"
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Authorization policies - dynamic from permissions
        services.AddAuthorization();

        // Rate limiting
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(15)
                    }));

            options.AddPolicy("sync", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("WebAdmin", policy =>
            {
                policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // OpenAPI
        services.AddOpenApi();
        
        // Infrastructure
        services.AddInfrastructure(configuration);

        // HttpClient for sync service to call central API
        services.AddHttpClient("CentralApi", client =>
        {
            client.BaseAddress = new Uri(configuration["CentralApi:BaseUrl"] ?? "http://localhost:5000");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseCors("WebAdmin");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready");

        return app;
    }
}