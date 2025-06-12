using GroceryChef.Api.DTOs.Auth;
using GroceryChef.Api.Entities;

namespace GroceryChef.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto, DateTime createdAtUtc) =>
        User.Create(dto.Email, dto.Name, createdAtUtc);

}
