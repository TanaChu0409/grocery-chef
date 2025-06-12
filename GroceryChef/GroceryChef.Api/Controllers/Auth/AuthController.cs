using GroceryChef.Api.Clock;
using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.Auth;
using GroceryChef.Api.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GroceryChef.Api.Controllers.Auth;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext applicationDbContext,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserDto registerUserDto,
        [FromServices] IDateTimeProvider dateTimeProvider)
    {
        using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
        applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        var identityUser = new IdentityUser
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.Email,
        };

        IdentityResult identityResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);

        if (!identityResult.Succeeded)
        {
            var extensions = new Dictionary<string, object?>
            {
                {
                    "errors",
                    identityResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                }
            };

            logger.LogError("{@Extensions}", extensions);

            return Problem(
                detail: "Unable to register user, please try again",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Entities.User user = registerUserDto.ToEntity(dateTimeProvider.UtcNow);
        user.SetIdentityId(identityUser.Id);

        applicationDbContext.Users.Add(user);

        await applicationDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(user.Id);
    }
}
