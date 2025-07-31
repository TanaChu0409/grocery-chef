using System.ComponentModel.DataAnnotations;

namespace GroceryChefClient.UI.Dtos.Auth;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Email can't be empty")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password can't be empty")]
    public string Password { get; set; }

    public LoginUserDto ToDto() =>
        new()
        {
            Email = Email,
            Password = Password
        };
}
