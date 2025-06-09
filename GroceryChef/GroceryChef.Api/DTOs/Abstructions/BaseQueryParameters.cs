using GroceryChef.Api.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.DTOs.Abstructions;

public abstract record BaseQueryParameters: AcceptHeaderDto
{
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
