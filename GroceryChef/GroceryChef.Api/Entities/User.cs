using System.Linq.Expressions;
using GroceryChef.Api.DTOs.Users;

namespace GroceryChef.Api.Entities;

public sealed class User
{
    private User()
    {
    }

    public string Id { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// We'll use this to store the IdentityId from the Identity Provider.
    /// This could be any identity provider like Azure AD, Cognito, Keycloak, Auth0, etc.
    /// </summary>
    public string IdentityId { get; private set; }

    public static User Create(
        string email,
        string name,
        DateTime createdAtUtc) =>
        new()
        {
            Id = $"u_{Ulid.NewUlid()}",
            Email = email,
            Name = name,
            CreatedAtUtc = createdAtUtc
        };
    public void Update(
        string name,
        DateTime updatedAtUtc)
    {
        Name = name;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SetIdentityId(string identityId)
    {
        IdentityId = identityId;
    }

    public static Expression<Func<User, UserDto>> ProjectToDto() =>
        u => u.ToDto();

    public UserDto ToDto() =>
        new()
        {
            Id = Id,
            Email = Email,
            Name = Name,
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc
        };
}
