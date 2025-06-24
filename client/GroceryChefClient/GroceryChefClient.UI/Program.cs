using GroceryChefClient.UI;
using GroceryChefClient.UI.Service;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => 
    new HttpClient 
    { 
        BaseAddress = new Uri("https://localhost:9001"),
        DefaultRequestHeaders =
        {
            { "Accept", "application/vnd.grocerychef.hateoas.1+json" }
        }
    });

builder.Services.AddTransient<IngredientService>();

await builder.Build().RunAsync();
