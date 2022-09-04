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

				var authCodeFlow = new OpenApiOAuthFlows
				{
					AuthorizationCode = new OpenApiOAuthFlow
					{
						AuthorizationUrl = GetAzureAdEndpoint(settings, "authorize"),
						TokenUrl = GetAzureAdEndpoint(settings, "token"),
						RefreshUrl = GetAzureAdEndpoint(settings, "refresh")
					}
				};
				authCodeFlow.AuthorizationCode.Scopes.Add(settings.OAuth.Scope, "Access This API");
				
				s.AddSecurity(
					Microsoft.Identity.Web.Constants.Bearer,
					new OpenApiSecurityScheme
					{
						Type = OpenApiSecuritySchemeType.OAuth2,
						Description = "Identity Server auth",
						Flow = OpenApiOAuth2Flow.AccessCode,
						Flows = authCodeFlow
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
					ClientId = settings.OAuth.ClientId,
					ClientSecret = string.Empty,
					UsePkceWithAuthorizationCodeGrant = true
				};
				cfg.OAuth2Client.Scopes.Add(settings.OAuth.Scope);
			});

		return app;
	}

	private static string GetAzureAdEndpoint(AppSettings settings, string endpointSuffix)
		=> $"{settings.OAuth.Authority}/connect/{endpointSuffix}";
}