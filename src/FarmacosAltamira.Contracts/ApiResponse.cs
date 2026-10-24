namespace FarmacosAltamira.Contracts;

public sealed record ApiResponse<T>(bool Exito, T? Dato, string? Mensaje = null, string? Codigo = null)
{
    public static ApiResponse<T> Ok(T dato, string? mensaje = null) => new(true, dato, mensaje);
    public static ApiResponse<T> Fail(string mensaje, string? codigo = null) => new(false, default, mensaje, codigo);
}

public sealed record PaginatedResponse<T>(IReadOnlyList<T> Items, long Total, int Pagina, int TamanoPagina)
{
    public int TotalPaginas => TamanoPagina > 0 ? (int)Math.Ceiling((double)Total / TamanoPagina) : 0;
    public bool TieneSiguiente => Pagina < TotalPaginas;
    public bool TieneAnterior => Pagina > 1;
}

public record PaginationRequest(int Pagina = 1, int TamanoPagina = 20)
{
    public int Pagina { get; init; } = Math.Max(1, Pagina);
    public int TamanoPagina { get; init; } = Math.Clamp(TamanoPagina, 1, 100);
    public int Offset => (Pagina - 1) * TamanoPagina;
}

public sealed record ErrorResponse(string Codigo, string Mensaje, object? Detalles = null);