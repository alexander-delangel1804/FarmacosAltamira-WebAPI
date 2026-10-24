using System.Security.Claims;
using System.Collections.Generic;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string role, int? locationId, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}