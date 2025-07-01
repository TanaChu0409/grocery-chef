namespace GroceryChef.Api.Settings;

public sealed class CorsOptions
{
    public const string PolicyName = "GroceryChefCorsPolicy";
    public const string SectionName = "Cors";

    public required string[] AllowedOrigins { get; init; }
}
