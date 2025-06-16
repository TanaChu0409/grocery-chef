using Asp.Versioning;
using GroceryChef.Api.Services;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using GroceryChef.Api.Database;
using GroceryChef.Api.Clock;
using GroceryChef.Api.DTOs.Carts;
using GroceryChef.Api.Services.Sorting;
using GroceryChef.Api.Entities;
using Microsoft.EntityFrameworkCore;
using GroceryChef.Api.DTOs.Common;
using System.Dynamic;
using GroceryChef.Api.Controllers.Recipes;
using GroceryChef.Api.DTOs.Recipes;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace GroceryChef.Api.Controllers.Carts;

[Authorize(Roles = Roles.Member)]
[ApiController]
[Route("carts")]
[ApiVersion(1.0)]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1)]
public sealed class CartController(
    ApplicationDbContext dbContext,
    LinkService linkService,
    UserContext userContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCarts(
        [FromQuery] CartQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataSharpingService dataSharpingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!sortMappingProvider.ValidateMappings<CartDto, Cart>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'.");
        }

        if (!dataSharpingService.Validate<CartDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        }

        query.Search ??= query.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<CartDto, Cart>();

        IQueryable<CartDto> cartQuery = dbContext
            .Carts
            .Where(c => c.UserId == userId)
            .Where(c =>
                query.Search == null ||
                c.Name.ToLower().Contains(query.Search))
            .ApplySort(query.Sort, sortMappings)
            .Select(Cart.ProjectToDto());

        int totalCount = await cartQuery.CountAsync();

        List<CartDto> carts = await cartQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataSharpingService.ShapeCollectionData(
               carts,
               query.Fields,
                query.IncludeLinks ? r => CreateLinksForCart(r.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        if (query.IncludeLinks)
        {
            paginationResult.Links = CreateLinksForCarts(
                query,
                paginationResult.HasNextPage,
                paginationResult.HasPreviousPage);
        }

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCart(
        string id,
        [FromQuery] CartQueryParameters query,
        DataSharpingService dataSharpingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!dataSharpingService.Validate<CartDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        }

        CartWithIngredientsDto? cart = await dbContext
            .Carts
            .Include(c => c.Ingredients)
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(Cart.ProjectToDtoWithIngredients())
            .FirstOrDefaultAsync();

        if (cart is null)
        {
            return NotFound();
        }

        ExpandoObject shapedCart = dataSharpingService.ShapeData(
            cart,
            query.Fields);

        if (query.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForCart(id, query.Fields);
            shapedCart.TryAdd("links", links);
        }

        return Ok(shapedCart);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCart(
        [FromBody] CreateCartDto createCart,
        [FromHeader] AcceptHeaderDto acceptHeader,
        [FromServices] IValidator<CreateCartDto> validator,
        [FromServices] IDateTimeProvider dateTimeProvider)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await validator.ValidateAndThrowAsync(createCart);

        var cart = Cart.Create(
            createCart.Name,
            userId,
            dateTimeProvider.UtcNow);

        dbContext.Carts.Add(cart);

        await dbContext.SaveChangesAsync();

        CartDto cartDto = cart.ToDto();
        if (acceptHeader.IncludeLinks)
        {
            cartDto.Links = CreateLinksForCart(cart.Id, null);
        }

        return CreatedAtAction(
            nameof(GetCart),
            new { id = cart.Id },
            cartDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCart(
        string id,
        [FromBody] UpdateCartDto updateCart,
        [FromServices] IDateTimeProvider dateTimeProvider)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Cart? cart = await dbContext
            .Carts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cart is null)
        {
            return NotFound();
        }

        if (await dbContext.Carts.AnyAsync(c => 
            c.Name == updateCart.Name && 
            c.Id != id &&
            c.UserId == userId))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided name was duplicate: '{updateCart.Name}'");
        }

        cart.UpdateFromDto(updateCart, dateTimeProvider.UtcNow);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCart(string id)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Cart? cart = await dbContext
            .Carts
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cart is null)
        {
            return NotFound();
        }

        dbContext.Carts.Remove(cart);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForCarts(
        CartQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links = [
            linkService.Create(nameof(GetCarts), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
            }),
            linkService.Create(nameof(CreateCart), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(
                nameof(GetCarts),
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
                nameof(GetCarts),
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

    private List<LinkDto> CreateLinksForCart(
        string id,
        string? fields)
    {
        return
        [
            linkService.Create(nameof(GetCart), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateCart), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(DeleteCart), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(
                nameof(CartIngredientController.UpsertCartIngredients),
                "upsert-ingredients",
                HttpMethods.Put,
                new { cartId = id },
                CartIngredientController.Name),
        ];
    }
}
