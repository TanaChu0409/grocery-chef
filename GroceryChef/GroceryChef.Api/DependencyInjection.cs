using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using GroceryChef.Api.Clock;
using GroceryChef.Api.Database;
using GroceryChef.Api.DTOs.Carts;
using GroceryChef.Api.DTOs.Ingredients;
using GroceryChef.Api.DTOs.Recipes;
using GroceryChef.Api.Entities;
using GroceryChef.Api.Extensions;
using GroceryChef.Api.Middleware;
using GroceryChef.Api.Services;
using GroceryChef.Api.Services.Sorting;
using GroceryChef.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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

    public static WebApplicationBuilder AddRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>();
                    ProblemDetails problemDetails = problemDetailsFactory
                        .CreateProblemDetails(
                            context.HttpContext,
                            StatusCodes.Status429TooManyRequests,
                            "Too Many Requests",
                            detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.");

                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
                }
            };

            options.AddPolicy("default", httpContext =>
            {
                string identityId = httpContext.User.GetIdentityId() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(identityId))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(
                        identityId,
                        _ =>
                            new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 100,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 5,
                                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                                TokensPerPeriod = 25
                            });
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    "anonymous",
                    _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1)
                        });
            });
        });

        return builder;
    }
}
