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
						AuthorizationUrl = GetAzureAdEndpoint(settings,"authorize"),
						TokenUrl = GetAzureAdEndpoint(settings,"token"),
						Type = OpenApiSecuritySchemeType.OAuth2,
						Description = "Azure AD auth",
						Flow = OpenApiOAuth2Flow.AccessCode,
						Scopes = new Dictionary<string, string> { [settings.AzureAd.Scope] = "Access This API" }
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
					UsePkceWithAuthorizationCodeGrant = true,
					Scopes = { settings.AzureAd.Scope }	// Set selected scopes by default
				};
			});

		return app;
	}

	private static string GetAzureAdEndpoint(AppSettings settings, string endpointSuffix)
		=> $"{settings.AzureAd.Authority}/oauth2/v2.0/{endpointSuffix}";
}