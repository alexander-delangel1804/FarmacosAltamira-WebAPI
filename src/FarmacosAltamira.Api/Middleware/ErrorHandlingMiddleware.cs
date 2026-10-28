using System.Net;
using System.Text.Json;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FarmacosAltamira.Api.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, codigo, mensaje, detalles) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "AUTH_INVALID_TOKEN", exception.Message, (object?)null),
            KeyNotFoundException => (HttpStatusCode.NotFound, "NO_ENCONTRADO", exception.Message, (object?)null),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, "REGLA_NEGOCIO", exception.Message, (object?)null),
            ArgumentException => (HttpStatusCode.BadRequest, "DATOS_INVALIDOS", exception.Message, (object?)null),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "CONCURRENCIA", "El recurso fue modificado por otro usuario", (object?)null),
            PostgresException pgEx when pgEx.SqlState == "P0001" => (HttpStatusCode.UnprocessableEntity, "VENTA_CONTROLADO_SIN_REGISTRO", pgEx.MessageText, (object?)null),
            PostgresException pgEx when pgEx.SqlState == "23505" => (HttpStatusCode.Conflict, "DUPLICADO", "Ya existe un registro con esos datos", (object?)null),
            ValidationException vex => (HttpStatusCode.BadRequest, "SYNC_VALIDATION_ERROR", "Error de validación", vex.ValidationResult?.ToString()),
            _ => (HttpStatusCode.InternalServerError, "ERROR_INTERNO", "Error interno del servidor", (object?)null)
        };

        _logger.LogError(exception, "Error en {Path}: {Codigo} - {Mensaje}", context.Request.Path, codigo, mensaje);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var error = new ProblemDetailsResponse
        {
            Status = (int)statusCode,
            Title = GetTitleForStatus(statusCode),
            Detail = mensaje,
            Instance = context.Request.Path,
            Codigo = codigo,
            Detalles = detalles
        };

        var json = JsonSerializer.Serialize(error, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        await context.Response.WriteAsync(json);
    }

    private static string GetTitleForStatus(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.Forbidden => "Forbidden",
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
        HttpStatusCode.Conflict => "Conflict",
        HttpStatusCode.InternalServerError => "Internal Server Error",
        HttpStatusCode.ServiceUnavailable => "Service Unavailable",
        _ => "Error"
    };
}

public sealed record ProblemDetailsResponse
{
    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("detail")]
    public string Detail { get; init; } = string.Empty;

    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; init; } = string.Empty;

    [JsonPropertyName("detalles")]
    public object? Detalles { get; init; }
}