using FarmacosAltamira.Application.Seguridad.Dto;
using System.Threading;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string ipOrigen, CancellationToken ct);
}