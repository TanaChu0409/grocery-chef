using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using GroceryChefClient.UI.Extensions;
using Microsoft.AspNetCore.Components.Authorization;

namespace GroceryChefClient.UI.Service;

public sealed class AuthProvider(
    InMemoryTokenStore inMemoryTokenStore,
    HttpClient httpClient) 
    : AuthenticationStateProvider
{
    private readonly AuthenticationState anonymous = new(
        new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string tokenInLocalStorage = await inMemoryTokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(tokenInLocalStorage))
        {
            return anonymous;
        }

        List<Claim> claims = JwtParser.ParseClaimsFromJwt(tokenInLocalStorage);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "bearer",
            tokenInLocalStorage);

        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(claims, "jwt")));
    }

    public void NotifyUserAuthentication(string token)
    {
        List<Claim> claims = JwtParser.ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "jwt"));

        var authState = Task.FromResult(
            new AuthenticationState(authenticatedUser));

        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(anonymous);
        NotifyAuthenticationStateChanged(authState);
    }
}
