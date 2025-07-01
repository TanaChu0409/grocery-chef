using GroceryChefClient.UI.Dtos.Common;
using GroceryChefClient.UI.Dtos.Ingredients;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Web;

namespace GroceryChefClient.UI.Service;

public sealed class IngredientService(
    HttpClient httpClient,
    InMemoryTokenStore inMemoryTokenStore)
{
    private const string IngredientUri = "ingredients";

    public async Task<List<IngredientDto>> GetIngredients(IngredientQueryRequest queryRequest)
    {
        NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
        foreach (PropertyInfo prop in queryRequest.GetType().GetProperties())
        {
            object? value = prop.GetValue(queryRequest);
            if (value is not null)
            {
                query[prop.Name] = value.ToString();
            }
        }

        string uri = $"{IngredientUri}?{query}";
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());
        HttpResponseMessage response = await httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        PaginationResult<IngredientDto>? ingredientWithPagination = await response.Content.ReadFromJsonAsync<PaginationResult<IngredientDto>>();

        return ingredientWithPagination?.Items ?? [];
    }
}
