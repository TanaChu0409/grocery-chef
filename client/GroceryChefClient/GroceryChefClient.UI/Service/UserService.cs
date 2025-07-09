using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GroceryChefClient.UI.Dtos.Users;
using Microsoft.AspNetCore.Components.Authorization;

namespace GroceryChefClient.UI.Service;

public sealed class UserService(
    HttpClient httpClient,
    InMemoryTokenStore inMemoryTokenStore,
    AuthenticationStateProvider authenticationStateProvider)
{
    private const string UserUri = "users";

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.GetAsync($"{UserUri}/me");

            response.EnsureSuccessStatusCode();

            UserDto? user = await response.Content.ReadFromJsonAsync<UserDto>();

            if (user is null)
            {
                return default;
            }

            return user;
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
}
