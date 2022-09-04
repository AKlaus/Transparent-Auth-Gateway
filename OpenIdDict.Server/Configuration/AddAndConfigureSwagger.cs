using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace AK.OAuthSamples.OpenIdDict.Server.Configuration;

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
					AuthorizationUrl = GetAzureAdEndpoint("authorize"),
					TokenUrl = GetAzureAdEndpoint("token"),
					RefreshUrl = GetAzureAdEndpoint("refresh")
				}
			};
			authCodeFlow.AuthorizationCode.Scopes.Add(settings.Auth.Scope, "Access This API");
				
			s.AddSecurity(
				Microsoft.Identity.Web.Constants.Bearer,
				new OpenApiSecurityScheme
				{
					Type = OpenApiSecuritySchemeType.OAuth2,
					Description = "OAuth 2.0 Server",
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
				ClientId = settings.Auth.ClientId,
				ClientSecret = string.Empty,
				UsePkceWithAuthorizationCodeGrant = true,
				Scopes = { settings.Auth.Scope }
			};
		});

		return app;
	}

	private static string GetAzureAdEndpoint(string endpointSuffix) => $"/connect/{endpointSuffix}";
}