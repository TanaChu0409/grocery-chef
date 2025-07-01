using Blazored.LocalStorage;
using GroceryChefClient.UI;
using GroceryChefClient.UI.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(sp => 
    new HttpClient 
    { 
        BaseAddress = new Uri("https://localhost:9001"),
        DefaultRequestHeaders =
        {
            { "Accept", "application/vnd.grocerychef.hateoas.1+json" }
        }
    });

builder.Services.AddScoped<AuthenticationStateProvider, AuthProvider>();
builder.Services.AddScoped<InMemoryTokenStore>();

builder.Services.AddTransient<AuthService>();
builder.Services.AddTransient<IngredientService>();


builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
