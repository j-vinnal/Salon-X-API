using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp;

/// <summary>
///     Configures Swagger options.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _descriptionProvider;

    /// <summary>
    ///     Constructor for ConfigureSwaggerOptions.
    /// </summary>
    /// <param name="descriptionProvider">API version description provider.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider descriptionProvider)
    {
        _descriptionProvider = descriptionProvider;
    }

    /// <summary>
    ///     Configures SwaggerGen options.
    /// </summary>
    /// <param name="options">SwaggerGen options.</param>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _descriptionProvider.ApiVersionDescriptions)
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = $"API {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    Contact = new OpenApiContact
                    {
                        Name = "Jüri Vinnal",
                        Email = "juvinn@taltech.ee",
                        Url = new Uri("https://www.taltech.ee")
                    }
                }
            );

        options.CustomSchemaIds(t => t.FullName);

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        // To get authorization working
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "foo bar",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            }
        );

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });

        // Add ConflictingActionsResolver to handle conflicting routes
        options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    }
}