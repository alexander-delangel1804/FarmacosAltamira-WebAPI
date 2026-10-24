using System;
using System.Collections.Generic;
using FarmacosAltamira.Contracts;

namespace FarmacosAltamira.Application.Common.Dtos;

public record PaginationRequestWrapper : PaginationRequest
{
    public string? Search { get; set; }
}

public class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public PaginatedResponse(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
