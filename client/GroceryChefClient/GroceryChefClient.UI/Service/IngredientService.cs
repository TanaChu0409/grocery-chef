using GroceryChefClient.UI.Dtos.Ingredients;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.Json;
using System.Web;

namespace GroceryChefClient.UI.Service;

public sealed class IngredientService(HttpClient httpClient)
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
        HttpResponseMessage response = await httpClient.GetAsync(uri);
        
        response.EnsureSuccessStatusCode();
        
        string json = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<List<IngredientDto>>(json) ?? [];
    }
}
