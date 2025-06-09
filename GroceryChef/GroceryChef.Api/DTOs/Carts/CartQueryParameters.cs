using GroceryChef.Api.DTOs.Abstructions;
using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.DTOs.Carts;

public sealed record CartQueryParameters : BaseQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
}
