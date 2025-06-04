using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.DTOs.Abstructions;

public abstract record BaseQueryParameters
{
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    [FromHeader(Name = "Accept")]
    public string Accept { get; init; }
}
