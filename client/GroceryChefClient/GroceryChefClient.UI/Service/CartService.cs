using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using GroceryChefClient.UI.Dtos.Carts;
using GroceryChefClient.UI.Dtos.Common;
using GroceryChefClient.UI.Dtos.Recipes;
using GroceryChefClient.UI.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using GroceryChefClient.UI.Pages.Carts;
using System.Threading;

namespace GroceryChefClient.UI.Service;

public sealed class CartService(
    HttpClient httpClient,
    InMemoryTokenStore inMemoryTokenStore,
    AuthenticationStateProvider authenticationStateProvider)
{
    private const string CartUri = "carts";

    public async Task<PaginationResult<CartDto>> GetCarts(
        CartQueryRequest queryRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            NameValueCollection? query = HttpExtensions.GetQueryParameter(queryRequest);

            string uri = $"{CartUri}?{query}";
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync(uri, cancellationToken);

            response.EnsureSuccessStatusCode();

            PaginationResult<CartDto> cartResultWithPagination =
                await response.Content.ReadFromJsonAsync<PaginationResult<CartDto>>(cancellationToken);

            return cartResultWithPagination;
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();

            return new();
        }
        catch
        {
            throw;
        }
    }

    public async Task AddCart(CreateCartDto createCartDto)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                CartUri,
                createCartDto);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }

    public async Task<EditCartDto> GetCart(string id)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync($"{CartUri}/{id}");

            response.EnsureSuccessStatusCode();

            EditCartDto cart =
                await response.Content.ReadFromJsonAsync<EditCartDto>();

            return cart;
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();

            throw;
        }
        catch
        {
            throw;
        }
    }

    public async Task EditCart(EditCartDto editCartDto)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(
                $"{CartUri}/{editCartDto.Id}",
                editCartDto);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }

    public async Task DeleteCart(string id)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.DeleteAsync($"{CartUri}/{id}");

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<CartDetailOptions>> GetCartOptions()
    {
        try
        {
            string uri = $"{CartUri}";
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            PaginationResult<CartDto> cartResultWithPagination =
                await response.Content.ReadFromJsonAsync<PaginationResult<CartDto>>();

            return cartResultWithPagination?.Items
                .Select(cart =>
                    new CartDetailOptions
                    {
                        CartId = cart.Id,
                        CartName = cart.Name
                    })
                .ToList() ?? [];
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();

            return [];
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<CartDetailGridViewModel>> GetCartIngredients(string cartId)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync($"{CartUri}/{cartId}");

            response.EnsureSuccessStatusCode();

            CartWithIngredientsDto cartWithIngredientDto =
                await response.Content.ReadFromJsonAsync<CartWithIngredientsDto>();

            List<CartDetailGridViewModel> cartDetailGridViewModel = cartWithIngredientDto?.Ingredients
                .Select(ingredient => new CartDetailGridViewModel
                {
                    IngredientId = ingredient.IngredientId,
                    Name = ingredient.Name,
                    IsBought = ingredient.IsBought,
                    Quantity = ingredient.Quantity,
                })
                .ToList() ?? [];

            return cartDetailGridViewModel;
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();

            throw;
        }
        catch
        {
            throw;
        }
    }

    public async Task UpsertCartIngredients(string cartId, List<CartDetailGridViewModel> cartDetailGrids)
    {
        try
        {
            string uri = $"{CartUri}/{cartId}/ingredients";
            UpsertCartIngredientsDto upsertCart = new()
            {
                Details = cartDetailGrids
                    .Select(cartDetail => new UpsertCartIngredientsDetailDto
                    {
                        IngredientId = cartDetail.IngredientId,
                        Quantity = cartDetail.Quantity
                    }).ToList()
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(
                uri,
                upsertCart);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }

    public async Task SetBoughtForIngredient(string cartId, string ingredientId)
    {
        try
        {
            string uri = $"{CartUri}/{cartId}/ingredients/{ingredientId}/bought";

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PatchAsync(uri, null);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }

    public async Task SetUnBoughtForIngredient(string cartId, string ingredientId)
    {
        try
        {
            string uri = $"{CartUri}/{cartId}/ingredients/{ingredientId}/unbought";

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PatchAsync(uri, null);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }

    public async Task DeleteIngredientFromCart(string cartId, string ingredientId)
    {
        try
        {
            string uri = $"{CartUri}/{cartId}/ingredients/{ingredientId}";

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.DeleteAsync(uri);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }
}
