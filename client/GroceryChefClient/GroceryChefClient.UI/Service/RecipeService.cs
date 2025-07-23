using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GroceryChefClient.UI.Dtos.Common;
using GroceryChefClient.UI.Dtos.Recipes;
using GroceryChefClient.UI.Extensions;
using Microsoft.AspNetCore.Components.Authorization;

namespace GroceryChefClient.UI.Service;

public sealed class RecipeService(
    HttpClient httpClient,
    InMemoryTokenStore inMemoryTokenStore,
    AuthenticationStateProvider authenticationStateProvider)
{
    private const string RecipeUri = "recipes";

    public async Task<List<RecipeDto>> GetRecipes(
        RecipeQueryRequest queryRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            NameValueCollection? query = HttpExtensions.GetQueryParameter(queryRequest);

            string uri = $"{RecipeUri}?{query}";
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync(uri, cancellationToken);

            response.EnsureSuccessStatusCode();

            PaginationResult<RecipeDto> recipeWithPagination =
                await response.Content.ReadFromJsonAsync<PaginationResult<RecipeDto>>(cancellationToken);

            return recipeWithPagination?.Items ?? [];
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

    public async Task AddRecipe(CreateRecipeDto createRecipe)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"{RecipeUri}",
                createRecipe);

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

    public async Task<RecipeDto> GetRecipeAsync(string id)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync($"{RecipeUri}/{id}");

            response.EnsureSuccessStatusCode();

            RecipeDto? recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();

            if (recipe is null)
            {
                throw new Exception("Recipe not found");
            }

            return recipe;
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();

            return default;
        }
        catch
        {
            throw;
        }
    }

    public async Task UpdateRecipeAsync(string id, UpdateRecipeDto updateRecipe)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(
                $"{RecipeUri}/{id}",
                updateRecipe);

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

    public async Task DeleteRecipeAsync(string id)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.DeleteAsync($"{RecipeUri}/{id}");

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
