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
}
