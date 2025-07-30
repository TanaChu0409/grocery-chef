using System.Dynamic;
using System.Net.Mime;
using Asp.Versioning;
using FluentValidation;
using GroceryChef.Api.Clock;
using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.Common;
using GroceryChef.Api.DTOs.Ingredients;
using GroceryChef.Api.Entities;
using GroceryChef.Api.Services;
using GroceryChef.Api.Services.Sorting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace GroceryChef.Api.Controllers.Ingredients;

[EnableRateLimiting("default")]
//[ResponseCache(Duration = 120)]
[Authorize(Roles = Roles.Member)]
[ApiController]
[Route("ingredients")]
[ApiVersion(1.0)]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1)]
public sealed class IngredientController(
    ApplicationDbContext dbContext,
    LinkService linkService,
    IDateTimeProvider dateTimeProvider,
    UserContext userContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetIngredients(
        [FromQuery] IngredientQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataSharpingService dataSharpingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!sortMappingProvider.ValidateMappings<IngredientDto, Ingredient>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'.");
        }

        if (!dataSharpingService.Validate<IngredientDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        }

        query.Search ??= query.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<IngredientDto, Ingredient>();

        IQueryable<IngredientDto> ingredientQuery = dbContext
            .Ingredients
            .Where(i => i.UserId == userId)
            .Where(i =>
                query.Search == null ||
                i.Name.ToLower().Contains(query.Search))
            .Where(i =>
                query.IsAllergy == null ||
                i.IsAllergy == query.IsAllergy.Value)
            .Where(i =>
                query.ShelfLifeOfDate == null ||
                i.ShelfLifeOfDate == query.ShelfLifeOfDate.Value)
            .ApplySort(query.Sort, sortMappings)
            .Select(Ingredient.ProjectToDto());

        int totalCount = await ingredientQuery.CountAsync();

        List<IngredientDto> ingredients = await ingredientQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataSharpingService.ShapeCollectionData(
                ingredients,
                query.Fields,
                query.IncludeLinks ? i => CreateLinksForIngredient(i.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        if (query.IncludeLinks)
        {
            paginationResult.Links = CreateLinksForIngredients(
                query,
                paginationResult.HasNextPage,
                paginationResult.HasPreviousPage);
        }

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIngredient(
        string id,
        [FromQuery] IngredientQueryParameters query,
        DataSharpingService dataSharpingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!dataSharpingService.Validate<IngredientDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        }

        IngredientDto? ingredient = await dbContext
            .Ingredients
            .Where(i => i.Id == id && i.UserId == userId)
            .Select(Ingredient.ProjectToDto())
            .FirstOrDefaultAsync();

        if (ingredient is null)
        {
            return NotFound();
        }

        if (query.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForIngredient(id, query.Fields);
            ingredient.Links = links;
        }

        ExpandoObject shapedIngredient = dataSharpingService.ShapeData(
            ingredient,
            query.Fields);

        return Ok(shapedIngredient);
    }

    [HttpPost]
    public async Task<ActionResult<IngredientDto>> CreateIngredient(
        [FromBody] CreateIngredientDto createIngredient,
        [FromHeader] AcceptHeaderDto acceptHeader,
        [FromServices] IValidator<CreateIngredientDto> validator)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await validator.ValidateAndThrowAsync(createIngredient);

        var ingredient = Ingredient.Create(
            createIngredient.Name,
            createIngredient.ShelfLifeOfDate,
            createIngredient.IsAllergy,
            userId,
            dateTimeProvider.UtcNow);

        dbContext.Ingredients.Add(ingredient);

        await dbContext.SaveChangesAsync();

        IngredientDto ingredientDto = ingredient.ToDto();
        if (acceptHeader.IncludeLinks)
        {
            ingredientDto.Links = CreateLinksForIngredient(ingredient.Id, null);
        }

        return CreatedAtAction(
            nameof(GetIngredient),
            new { id = ingredientDto.Id },
            ingredientDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIngredient(
        string id,
        UpdateIngredientDto updateIngredient,
        InMemoryETagStore eTagStore)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Ingredient? ingredient = await dbContext
            .Ingredients
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (ingredient is null)
        {
            return NotFound();
        }

        ingredient.UpdateFromDto(updateIngredient);

        await dbContext.SaveChangesAsync();
        eTagStore.SetETag(Request.Path.Value!, ingredient.ToDto());

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchIngredient(
        string id,
        JsonPatchDocument<IngredientDto> patchDocument)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Ingredient? ingredient = await dbContext
            .Ingredients
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (ingredient is null)
        {
            return NotFound();
        }

        IngredientDto ingredientDto = ingredient.ToDto();

        patchDocument.ApplyTo(ingredientDto, ModelState);

        if (!TryValidateModel(ingredientDto))
        {
            return ValidationProblem(ModelState);
        }

        ingredient.UpdateFromDto(
            new UpdateIngredientDto
            {
                Name = ingredientDto.Name,
                ShelfLifeOfDate = ingredientDto.ShelfLifeOfDate,
                IsAllergy = ingredientDto.IsAllergy
            });

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIngredient(string id)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Ingredient? ingredient = await dbContext
            .Ingredients
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (ingredient is null)
        {
            return NotFound();
        }

        dbContext.Ingredients.Remove(ingredient);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForIngredients(
        IngredientQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links = [
            linkService.Create(nameof(GetIngredients), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                isAllergy = parameters.IsAllergy,
            }),
            linkService.Create(nameof(CreateIngredient), "create", HttpMethods.Post),
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(
                nameof(GetIngredients),
                "next-page",
                HttpMethods.Get,
                new
                {
                    page = parameters.Page + 1,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                    isAllergy = parameters.IsAllergy,
                }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(
                nameof(GetIngredients),
                "previous-page",
                HttpMethods.Get,
                new
                {
                    page = parameters.Page - 1,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                    isAllergy = parameters.IsAllergy,
                }));
        }

        return links;
    }

    private List<LinkDto> CreateLinksForIngredient(string id, string? fields)
    {
        return
        [
            linkService.Create(nameof(GetIngredient), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateIngredient), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchIngredient), "partial-update", HttpMethods.Patch, new { id }),
            linkService.Create(nameof(DeleteIngredient), "delete", HttpMethods.Delete, new { id }),
        ];
    }
}
