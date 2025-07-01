using System.Text.Json.Serialization;

namespace GroceryChefClient.UI.Dtos.Common;

public abstract record BaseQueryRequest
{
    [JsonPropertyName("q")]
    public string? Search { get; set; }
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string Email { get; init; }
}
