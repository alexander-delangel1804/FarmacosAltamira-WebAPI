using System.Net;
using System.Text.Json;
using FarmacosAltamira.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

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
        var (statusCode, codigo, mensaje) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "NO_AUTORIZADO", exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "NO_ENCONTRADO", exception.Message),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, "REGLA_NEGOCIO", exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "DATOS_INVALIDOS", exception.Message),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "CONCURRENCIA", "El recurso fue modificado por otro usuario"),
            PostgresException pgEx when pgEx.SqlState == "P0001" => (HttpStatusCode.UnprocessableEntity, "VENTA_CONTROLADO_SIN_REGISTRO", pgEx.MessageText),
            PostgresException pgEx when pgEx.SqlState == "23505" => (HttpStatusCode.Conflict, "DUPLICADO", "Ya existe un registro con esos datos"),
            _ => (HttpStatusCode.InternalServerError, "ERROR_INTERNO", "Error interno del servidor")
        };

        _logger.LogError(exception, "Error en {Path}: {Codigo} - {Mensaje}", context.Request.Path, codigo, mensaje);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var error = new ErrorResponse(codigo, mensaje);
        var json = JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}