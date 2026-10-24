using FarmacosAltamira.Application.Catalogo.Dto;
using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmacosAltamira.Application.Catalogo.Services;

public sealed class CategoriaService
{
    private readonly ICentralDbContext _db;

    public CategoriaService(ICentralDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken ct)
    {
        return await _db.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto(c.IdCategoria, c.Nombre))
            .ToListAsync(ct);
    }
}