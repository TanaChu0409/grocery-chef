using GroceryChef.Api;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddApiService()
    .AddErrorHandling()
    .AddObservability()
    .AddApplicationServices();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
