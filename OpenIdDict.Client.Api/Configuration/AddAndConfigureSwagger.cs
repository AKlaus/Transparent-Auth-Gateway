using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;

internal static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddAndConfigureSwagger(this IServiceCollection services, AppSettings settings)
	{
		services.AddEndpointsApiExplorer();
		services.AddOpenApiDocument(s =>
			{
				s.Title = settings.AppName;
				
				s.AddSecurity(
					"Bearer",
					new OpenApiSecurityScheme
					{
						Type = OpenApiSecuritySchemeType.OAuth2,
						Description = "Identity Server auth",
						Flows = new OpenApiOAuthFlows
						{
							ClientCredentials = new OpenApiOAuthFlow
							{
								TokenUrl = GetAuthEndpoint(settings, "token")
							},
							AuthorizationCode = new OpenApiOAuthFlow
							{
								TokenUrl = GetAuthEndpoint(settings, "token"),
								AuthorizationUrl = GetAuthEndpoint(settings, "authorize"),
								RefreshUrl = GetAuthEndpoint(settings, "token"),
								Scopes = new Dictionary<string, string> { [settings.OAuth.Scope] = "Access This API" }
							}
						}
					});
				s.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(/* 'Bearer' is the default scheme */));
			});

		return services;
	}
	
	public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app, AppSettings settings)
	{
		app.UseOpenApi();
		app.UseSwaggerUi(cfg =>
			{
				cfg.OAuth2Client = new OAuth2ClientSettings
				{
					AppName = settings.AppName,
					ClientId = settings.OAuth.ClientId,
					UsePkceWithAuthorizationCodeGrant = true
				};
				// Set selected scopes by default
				cfg.OAuth2Client.Scopes.Add(settings.OAuth.Scope);
			});

		return app;
	}

	private static string GetAuthEndpoint(AppSettings settings, string endpointSuffix)
		=> $"{settings.OAuth.Authority}/connect/{endpointSuffix}";
}