using GroceryChef.Api;
using GroceryChef.Api.Extensions;
using GroceryChef.Api.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddApiService()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices()
    .AddAuthenticationService()
    .AddRateLimiting();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();

    await app.SeedInitialDataAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

//app.UseMiddleware<ETagMiddleware>();

app.MapControllers();

await app.RunAsync();
