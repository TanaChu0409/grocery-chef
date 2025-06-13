using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.CartsIngredients;
using GroceryChef.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryChef.Api.Controllers.Carts;

[ApiController]
[Route("carts/{cartId}/ingredients")]
[Authorize]
public sealed class CartIngredientController(ApplicationDbContext dbContext) : ControllerBase
{
    public static readonly string Name = nameof(CartIngredientController)
        .Replace("Controller", string.Empty);

    [HttpPut]
    public async Task<ActionResult> UpsertCartIngredients(
        string cartId,
        UpsertCartIngredientsDto upsertCartIngredients)
    {
        var upsertIngredientIds = upsertCartIngredients.Details
            .Select(x => x.IngredientId)
            .ToList();

        Cart? cart = await dbContext
            .Carts
            .Include(c => c.Ingredients)
            .FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart is null)
        {
            return NotFound();
        }

        var currentIngredientIds = cart
            .CartIngredients
            .Select(ci => ci.IngredientId)
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
            return BadRequest("One or more ingredient IDs is invalid");
        }

        cart.RemoveAllCartIngredients(upsertIngredientIds);

        var cartIngredientsToAdd = upsertCartIngredients
            .Details
            .Where(x => !currentIngredientIds.Contains(x.IngredientId))
            .ToList();

        cart.AddCartIngredients(cartIngredientsToAdd);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{ingredientId}/bought")]
    public async Task<IActionResult> SetBoughtForIngredient(
        string cartId,
        string ingredientId)
    {
        CartIngredient? cartIngredient = await dbContext
            .CartIngredients
            .FirstOrDefaultAsync(ci =>
                ci.CartId == cartId &&
                ci.IngredientId == ingredientId);

        if (cartIngredient is null)
        {
            return NotFound();
        }

        cartIngredient.IsBought = true;

        dbContext.CartIngredients.Update(cartIngredient);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{ingredientId}")]
    public async Task<ActionResult> DeleteIngredient(string cartId, string ingredientId)
    {
        CartIngredient? cartIngredient = await dbContext
            .CartIngredients
            .FirstOrDefaultAsync(ci =>
                ci.CartId == cartId &&
                ci.IngredientId == ingredientId);

        if (cartIngredient is null)
        {
            return NotFound();
        }

        dbContext.CartIngredients.Remove(cartIngredient);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
