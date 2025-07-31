using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using GroceryChefClient.UI.Dtos.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace GroceryChefClient.UI.Service;

public sealed class AuthService(
    HttpClient httpClient,
    InMemoryTokenStore inMemoryTokenStore,
    AuthenticationStateProvider authenticationStateProvider)
{
    private const string AuthUri = "auth";

    public async Task<bool> LoginAsync(LoginUserDto loginUser)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{AuthUri}/login", loginUser);
            response.EnsureSuccessStatusCode();

            AccessTokensDto? tokens = await response.Content.ReadFromJsonAsync<AccessTokensDto>();
            if (tokens is null)
            {
                return false;
            }

            await inMemoryTokenStore.SetTokenAsync(tokens.AccessToken);
            await inMemoryTokenStore.SetRefreshTokenAsync(tokens.RefreshToken);

            ((AuthProvider)authenticationStateProvider).NotifyUserAuthentication(tokens.AccessToken);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "bearer",
                tokens.AccessToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RefreshAsync()
    {
        try
        {
            string refreshToken = await inMemoryTokenStore.GetRefreshTokenAsync();

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"{AuthUri}/refresh", 
                new RefreshTokenDto
                {
                    RefreshToken = refreshToken
                });

            response.EnsureSuccessStatusCode();
            AccessTokensDto? tokens = await response.Content.ReadFromJsonAsync<AccessTokensDto>();

            if (tokens is null)
            {
                return false;
            }

            await inMemoryTokenStore.SetTokenAsync(tokens.AccessToken);
            await inMemoryTokenStore.SetRefreshTokenAsync(tokens.RefreshToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await inMemoryTokenStore.RemoveAllTokenAsync();
        ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        httpClient.DefaultRequestHeaders.Authorization = null;
    } 

    public async Task RegisterAsync(RegisterDto registerDto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{AuthUri}/register",
            registerDto);
        response.EnsureSuccessStatusCode();
    }
}
