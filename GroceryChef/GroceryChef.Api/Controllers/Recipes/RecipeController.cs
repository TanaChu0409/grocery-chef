using System.Dynamic;
using System.Net.Mime;
using Asp.Versioning;
using FluentValidation;
using GroceryChef.Api.Clock;
using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.Common;
using GroceryChef.Api.DTOs.Recipes;
using GroceryChef.Api.Entities;
using GroceryChef.Api.Services;
using GroceryChef.Api.Services.Sorting;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryChef.Api.Controllers.Recipes;

[ApiController]
[Route("recipes")]
[ApiVersion(1.0)]
public sealed class RecipeController(
    ApplicationDbContext dbContext,
    LinkService linkService,
    IDateTimeProvider dateTimeProvider) : ControllerBase
{
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.Application.HateoasJsonV1)]
    public async Task<IActionResult> GetRecipes(
        [FromQuery] RecipeQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataSharpingService dataSharpingService)
    {
        if (!sortMappingProvider.ValidateMappings<RecipeDto, Recipe>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'.");
        }

        if (!dataSharpingService.Validate<RecipeDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        }

        query.Search ??= query.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<RecipeDto, Recipe>();

        IQueryable<RecipeDto> recipeQuery = dbContext
            .Recipes
            .Where(r =>
                query.Search == null ||
                r.Name.ToLower().Contains(query.Search) ||
                r.Content.ToLower().Contains(query.Search))
            .Where(r => !r.IsArchived)
            .ApplySort(query.Sort, sortMappings)
            .Select(Recipe.ProjectToDto());

        int totalCount = await recipeQuery.CountAsync();

        List<RecipeDto> recipes = await recipeQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        bool includeLinks = query.Accept == CustomMediaTypeNames.Application.HateoasJsonV1;
        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataSharpingService.ShapeCollectionData(
                recipes,
                query.Fields,
                includeLinks ? r => CreateLinksForRecipe(r.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForRecipes(
                query,
                paginationResult.HasNextPage,
                paginationResult.HasPreviousPage);
        }

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.Application.HateoasJsonV1)]
    public async Task<IActionResult> GetRecipe(
        string id,
        string? fields,
        [FromHeader(Name = "Accept")] string? accept,
        DataSharpingService dataSharpingService)
    {
        if (!dataSharpingService.Validate<RecipeDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{fields}'");
        }

        RecipeWithIngredientsDto? recipe = await dbContext
            .Recipes
            .Include(r => r.Ingredients)
            .Where(r => r.Id == id)
            .Select(Recipe.ProjectToDtoWithIngredients())
            .FirstOrDefaultAsync();

        if (recipe is null)
        {
            return NotFound();
        }

        ExpandoObject shapedRecipe = dataSharpingService.ShapeData(recipe, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJsonV1)
        {
            List<LinkDto> links = CreateLinksForRecipe(id, fields);
            shapedRecipe.TryAdd("links", links);
        }

        return Ok(shapedRecipe);
    }

    [HttpPost]
    public async Task<ActionResult> CreateRecipe(
        [FromBody] CreateRecipeDto createRecipe,
        [FromServices] IValidator<CreateRecipeDto> validator)
    {
        await validator.ValidateAndThrowAsync(createRecipe);

        var recipe = Recipe.Create(
            createRecipe.Name,
            createRecipe.Description,
            createRecipe.Content,
            dateTimeProvider.UtcNow);

        dbContext.Recipes.Add(recipe);

        await dbContext.SaveChangesAsync();

        RecipeDto recipeDto = recipe.ToDto();
        recipeDto.Links = CreateLinksForRecipe(recipe.Id, null);

        return CreatedAtAction(
            nameof(GetRecipe),
            new { id = recipe.Id },
            recipeDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRecipe(
        string id,
        [FromBody] UpdateRecipeDto updateRecipe)
    {
        Recipe? recipe = await dbContext
            .Recipes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe is null)
        {
            return NotFound();
        }

        recipe.UpdateFromDto(updateRecipe);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchRecipe(
        string id,
        JsonPatchDocument<RecipeDto> patchDocument)
    {
        Recipe? recipe = await dbContext
            .Recipes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe is null)
        {
            return NotFound();
        }

        RecipeDto recipeDto = recipe.ToDto();

        patchDocument.ApplyTo(recipeDto, ModelState);

        if (!TryValidateModel(recipeDto))
        {
            return ValidationProblem(ModelState);
        }

        recipe.UpdateFromDto(
            new UpdateRecipeDto
            {
                Name = recipeDto.Name,
                Content = recipeDto.Content,
                Description = recipeDto.Description
            });

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipe(string id)
    {
        Recipe? recipe = await dbContext
            .Recipes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe is null)
        {
            return NotFound();
        }

        recipe.Archived();

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForRecipes(
        RecipeQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links = [
            linkService.Create(nameof(GetRecipes), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
            }),
            linkService.Create(nameof(CreateRecipe), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(
                nameof(GetRecipes),
                "next-page",
                HttpMethods.Get,
                new
                {
                    page = parameters.Page + 1,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(
                nameof(GetRecipes),
                "previous-page",
                HttpMethods.Get,
                new
                {
                    page = parameters.Page - 1,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                }));
        }

        return links;
    }

    private List<LinkDto> CreateLinksForRecipe(
        string id,
        string? fields)
    {
        return
        [
            linkService.Create(nameof(GetRecipe), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateRecipe), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchRecipe), "partial-update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(DeleteRecipe), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(
                nameof(RecipeIngredientsController.UpsertRecipeIngredients),
                "upsert-ingredients",
                HttpMethods.Put,
                new { recipeId = id },
                RecipeIngredientsController.Name)
        ];
    }
}
