using FarmacosAltamira.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmacosAltamira.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class AuditarAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _modulo;
    private readonly string _accion;

    public AuditarAttribute(string modulo, string accion)
    {
        _modulo = modulo;
        _accion = accion;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (resultContext.Exception == null && resultContext.Result is ObjectResult { StatusCode: >= 200 and < 400 })
        {
            var auditoria = context.HttpContext.RequestServices.GetService<IAuditoria>();
            if (auditoria != null)
            {
                var userId = context.HttpContext.User.FindFirst("sub")?.Value;
                var ubicacionId = context.HttpContext.User.FindFirst("ubi")?.Value;

                if (int.TryParse(userId, out var idUsuario))
                {
                    var entidad = context.ActionArguments.Keys.FirstOrDefault();
                    var idEntidad = context.ActionArguments.Values.FirstOrDefault()?.ToString();
                    var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

                    await auditoria.RegistrarAsync(
                        idUsuario,
                        int.TryParse(ubicacionId, out var ubi) ? ubi : null,
                        _modulo,
                        _accion,
                        entidad,
                        idEntidad,
                        ipOrigen: ip);
                }
            }
        }
    }
}