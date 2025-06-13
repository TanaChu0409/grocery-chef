using Microsoft.AspNetCore.Identity;

namespace GroceryChef.Api.Entities;

public sealed class RefreshToken
{
    public string Id { get; set; }
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime ExpiresAtUtc { get; set; }

    public IdentityUser User { get; set; }
}
