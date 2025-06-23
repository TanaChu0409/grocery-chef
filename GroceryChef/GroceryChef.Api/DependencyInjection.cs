using System.Text;
using Asp.Versioning;
using FluentValidation;
using GroceryChef.Api.Clock;
using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.Carts;
using GroceryChef.Api.DTOs.Ingredients;
using GroceryChef.Api.DTOs.Recipes;
using GroceryChef.Api.Entities;
using GroceryChef.Api.Middleware;
using GroceryChef.Api.Services;
using GroceryChef.Api.Services.Sorting;
using GroceryChef.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GroceryChef.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiService(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;
        })
        .AddNewtonsoftJson(options =>
            options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver());

        builder.Services.Configure<MvcOptions>(options =>
        {
            NewtonsoftJsonOutputFormatter formatters = options.OutputFormatters
                .OfType<NewtonsoftJsonOutputFormatter>()
                .First();

            formatters.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            formatters.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
        });

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new DefaultApiVersionSelector(options);

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new MediaTypeApiVersionReader(),
                    new MediaTypeApiVersionReaderBuilder()
                        .Template("application/vnd.grocerychef.hateoas.{version}+json")
                        .Build());
            })
            .AddMvc();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GroceryChef API",
                Version = "v1",
                Description = "API for GroceryChef application",
            });

            options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
        });

        builder.Services.AddResponseCaching();

        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                .UseSnakeCaseNamingConvention());

        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddNpgsql())
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddTransient<SortMappingProvider>();

        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<IngredientDto, Ingredient>>(_ =>
            Ingredient.SortMapping);
        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<RecipeDto, Recipe>>(_ =>
            Recipe.SortMapping);
        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<CartDto, Cart>>(_ =>
            Cart.SortMapping);

        builder.Services.AddTransient<DataSharpingService>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<LinkService>();
        builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        builder.Services.AddTransient<TokenProvider>();

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<UserContext>();

        builder.Services.AddSingleton<InMemoryETagStore>();

        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationService(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));

        JwtAuthOptions? jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions!.Issuer,
                    ValidAudience = jwtAuthOptions!.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions!.Key))
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}
