using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace AK.OAuthSamples.AzureADAuthClient.Configuration;

internal static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddAndConfigureSwagger(this IServiceCollection services, AppSettings settings)
	{
		services.AddEndpointsApiExplorer();
		services.AddOpenApiDocument(s =>
			{
				s.Title = settings.AppName;
				s.AddSecurity(
					Microsoft.Identity.Web.Constants.Bearer,
					new OpenApiSecurityScheme
					{
						AuthorizationUrl = GetAzureAdEndpoint(settings,"authorize")+"?nonce=SWAGGER",	// pass 'nonce', as Swagger UI still doesn't fully support OIDC (see https://github.com/swagger-api/swagger-ui/issues/3517)
																													// It also doesn't support `response_mode=form_post` but it's not critical for local debugging
						Type = OpenApiSecuritySchemeType.OAuth2,
						Description = "Azure AD auth by `id_token` only",
						Flow = OpenApiOAuth2Flow.Implicit,
						ExtensionData = new Dictionary<string, object> 
						{
							// Forcing to extract `id_token` and use it as the bearer token (see https://stackoverflow.com/a/59784134/968003)
							["x-tokenName"]= "id_token"
						},
						Scopes = new Dictionary<string, string>
						{
							[settings.AzureAd.Scope] = "Access This API",	// [Optional] To control access to the app by Azure AD users
							["profile"] = "Profile",						// [Optional] Returns claims that represent basic profile information, including name, family_name, given_name, etc.
							["openid"] = "Mandatory 'OpenId'"				// Required by OIDC
						}
					});
				s.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(Microsoft.Identity.Web.Constants.Bearer));
			});

		return services;
	}
	
	public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app, AppSettings settings)
	{
		app.UseOpenApi();
		app.UseSwaggerUi3(cfg =>
			{
				cfg.OAuth2Client = new OAuth2ClientSettings
				{
					AppName = settings.AppName,
					ClientId = settings.AzureAd.ClientId,
					ClientSecret = string.Empty,
					Scopes = { settings.AzureAd.Scope, "profile", "openid" }	// Set selected scopes by default
				};
				// As Swagger has the 'response_type' hard-coded to `token` for implicit flow (See https://github.com/swagger-api/swagger-ui/issues/3906 and https://github.com/RicoSuter/NSwag/issues/2302), 
				// We inject a special script that substitutes it to `id_token`.
				cfg.CustomJavaScriptPath = "customSwaggerUiAuth.js";
			});

		return app;
	}

	private static string GetAzureAdEndpoint(AppSettings settings, string endpointSuffix)
		=> $"{settings.AzureAd.Authority}/oauth2/v2.0/{endpointSuffix}";
}