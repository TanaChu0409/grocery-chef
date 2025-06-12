using Asp.Versioning;
using GroceryChef.Api.Services;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.Users;
using GroceryChef.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroceryChef.Api.Controllers.Users;

[ApiController]
[Route("users")]
[ApiVersion(1.0)]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1)]
public sealed class UsersController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        UserDto? user = await dbContext.Users
            .Where(u => u.Id == id)
            .Select(Entities.User.ProjectToDto())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
