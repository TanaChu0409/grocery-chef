using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Auth;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
    [Required(ErrorMessage = "Confirm Password is required.")]
    public string ConfirmPassword { get; set; }
    public bool IsPasswordConfirmed =>
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) && 
        Password == ConfirmPassword;

    public RegisterDto ToDto() =>
        new()
        {
            Email = Email,
            Name = Name,
            Password = Password,
            ConfirmPassword = ConfirmPassword
        };
}

public sealed record RegisterDto
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}
