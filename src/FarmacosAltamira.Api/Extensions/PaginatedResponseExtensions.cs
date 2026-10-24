namespace FarmacosAltamira.Api.Extensions;

public static class PaginatedResponseExtensions
{
    public static Contracts.PaginatedResponse<T> ToContract<T>(this Application.Common.Dtos.PaginatedResponse<T> source)
    {
        return new Contracts.PaginatedResponse<T>(source.Items, source.TotalCount, source.Page, source.PageSize);
    }
}