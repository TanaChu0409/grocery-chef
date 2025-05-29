using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.Controllers.Ingredients;

[ApiController]
[Route("ingredients")]
public sealed class IngredientController : ControllerBase
{
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<IActionResult> GetIngredients()
    {
        throw new NotImplementedException();
    }
}
