using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;

namespace AK.OAuthSamples.OpenIdDict.Server.Configuration;

internal static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddAndConfigureSwagger(this IServiceCollection services, AppSettings settings)
	{
		services.AddOpenApi(options =>
		{
			options.AddDocumentTransformer((document, _, _) =>
			{
				document.Info = new OpenApiInfo { Title = settings.AppName };
				return Task.CompletedTask;
			});
			options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
		});

		return services;
	}

	public static WebApplication ConfigureSwagger(this WebApplication app, AppSettings settings)
	{
		app.MapOpenApi();	// Can limit OpenAPI document access to authorized users by calling `RequireAuthorization()`
		
		app.UseSwaggerUI(cfg =>
		{
			cfg.SwaggerEndpoint("/openapi/v1.json", "v1");
			cfg.OAuthClientId(settings.Auth.ClientId);
			cfg.OAuthClientSecret("Cant_be_empty,but_dismissed_anyway");
			cfg.OAuthUsePkce();
		});

		return app;
	}

	/// <summary>
	///		OpenAPI document transformer to add authentication functionality to the end-points
	/// </summary>
	/// <remarks>
	///		Based on the official example from https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0#use-document-transformers
	/// </remarks>
	private sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
	{
		public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancelToken)
		{
			var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
			if (authenticationSchemes.Any(scheme => scheme.Name == OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme))
			{
				var requirements = new Dictionary<string, OpenApiSecurityScheme>
				{
					[Microsoft.Identity.Web.Constants.Bearer] = new()
					{
						Type = SecuritySchemeType.OAuth2,
						Description = "Identity Server auth",
						Flows = new OpenApiOAuthFlows
						{
							ClientCredentials = new OpenApiOAuthFlow
							{
								TokenUrl = GetAuthEndpoint("token")
							},
							AuthorizationCode = new OpenApiOAuthFlow
							{
								TokenUrl = GetAuthEndpoint("token"),
								AuthorizationUrl = GetAuthEndpoint("authorize"),
								RefreshUrl = GetAuthEndpoint("token"),
							}
						}
					}
				};
				document.Components ??= new OpenApiComponents();
				document.Components.SecuritySchemes = requirements;
				// Mark all the end-points with an option to authenticate 
				foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
				{
					operation.Value.Security.Add(new OpenApiSecurityRequirement
					{
						[new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = Microsoft.Identity.Web.Constants.Bearer, Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
					});
				}
			}
		}

		private static Uri GetAuthEndpoint(string endpointSuffix) => new($"/connect/{endpointSuffix}", UriKind.Relative);
	}
}