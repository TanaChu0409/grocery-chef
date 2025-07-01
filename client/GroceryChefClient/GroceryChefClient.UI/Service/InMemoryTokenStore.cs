using Blazored.LocalStorage;

namespace GroceryChefClient.UI.Service;

public sealed class InMemoryTokenStore(
    ILocalStorageService localStorageService)
{
    private const string Token = "token";
    private const string Refresh = "refresh";

    public async Task SetTokenAsync(string token) =>
        await localStorageService.SetItemAsStringAsync(Token, token);

    public async Task SetRefreshTokenAsync(string refreshToken) =>
        await localStorageService.SetItemAsStringAsync(Refresh, refreshToken);

    public async Task<string> GetTokenAsync() =>
        await localStorageService.GetItemAsStringAsync(Token);

    public async Task<string> GetRefreshTokenAsync() =>
        await localStorageService.GetItemAsStringAsync(Refresh);

    public async Task RemoveAllTokenAsync() => 
        await localStorageService.RemoveItemsAsync([Token, Refresh]);
}
