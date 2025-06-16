using GroceryChef.Api.Clock;
using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.RecipeIngredients;
using GroceryChef.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryChef.Api.Controllers.Recipes;

[Authorize(Roles = Roles.Member)]
[ApiController]
[Route("recipes/{recipeId}/ingredients")]
public sealed class RecipeIngredientsController(ApplicationDbContext dbContext) : ControllerBase
{
    public static readonly string Name = nameof(RecipeIngredientsController)
        .Replace("Controller", string.Empty);

    [HttpPut]
    public async Task<ActionResult> UpsertRecipeIngredients(
        string recipeId,
        UpsertRecipeIngredientsDto upsertRecipeIngredients,
        IDateTimeProvider dateTimeProvider)
    {
        var upsertIngredientIds = upsertRecipeIngredients.Details
            .Select(x => x.IngredientId)
            .ToList();

        Recipe? recipe = await dbContext
            .Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe is null)
        {
            return NotFound();
        }

        var currentIngredientIds = recipe
            .RecipeIngredients
            .Select(ri => ri.IngredientId)
            .ToHashSet();

        if (currentIngredientIds.SetEquals(upsertIngredientIds))
        {
            return NoContent();
        }

        List<string> existingIngredientIds = await dbContext
            .Ingredients
            .Where(i => upsertIngredientIds.Contains(i.Id))
            .Select(i => i.Id)
            .ToListAsync();

        if (existingIngredientIds.Count != upsertIngredientIds.Count)
        {
            return BadRequest("One or more tag IDs is invalid");
        }

        recipe.RemoveAllRecipeIngredients(upsertIngredientIds);

        var recipeIngredientsToAdd = upsertRecipeIngredients
            .Details
            .Where(x => !currentIngredientIds.Contains(x.IngredientId))
            .ToList();

        recipe.AddRecipeIngredients(
                recipeIngredientsToAdd,
                dateTimeProvider.UtcNow);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{ingredientId}")]
    public async Task<ActionResult> DeleteIngredient(string recipeId, string ingredientId)
    {
        RecipeIngredient? recipeIngredient = await dbContext
            .RecipeIngredients
            .FirstOrDefaultAsync(ri =>
                ri.RecipeId == recipeId &&
                ri.IngredientId == ingredientId);

        if (recipeIngredient is null)
        {
            return NotFound();
        }

        dbContext.RecipeIngredients.Remove(recipeIngredient);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
