using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace JWTAuth.Core.Swagger;

public static class SwaggerExtension
{
    public static IServiceCollection AddSwaggerService(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options => { 

            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new  OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        services.AddEndpointsApiExplorer();
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        return services;
    }

    public static IApplicationBuilder UseSwaggerService(this IApplicationBuilder app,  IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/openapi/{description.GroupName}.json", // fixed route
                    $"v{description.ApiVersion}");
            }
        });

        return app;
    }

    private static readonly string[] preferredSchemes = new[] { "Bearer" };

    public static WebApplication UseScalarService(this WebApplication app, IApiVersionDescriptionProvider provider)
    {
        app.MapOpenApi(); 
        app.MapScalarApiReference(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                Console.WriteLine($"Adding document for: {description.GroupName}");
                options.AddDocument(
                    description.GroupName,
                    $"v{description.ApiVersion}");

            }

            options.Title = "JWT Auth API";
            options.Theme = ScalarTheme.DeepSpace;
            options.DefaultHttpClient = new(ScalarTarget.JavaScript, ScalarClient.Fetch);
            options.AddPreferredSecuritySchemes(preferredSchemes).AddHttpAuthentication("Bearer", auth =>
            {
            
            });
        });
        return app;
    }
}