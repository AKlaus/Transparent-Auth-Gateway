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

				s.AddSecurity(
					Microsoft.Identity.Web.Constants.Bearer,
					new OpenApiSecurityScheme
					{
						AuthorizationUrl = GetAuthEndpoint("authorize"),
						TokenUrl = GetAuthEndpoint("token"),
						Type = OpenApiSecuritySchemeType.OAuth2,
						Description = "Identity Server auth",
						Flow = OpenApiOAuth2Flow.AccessCode,
						Scopes = settings.Auth.ScopesFullSet
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
					UsePkceWithAuthorizationCodeGrant = true
				};
				// Set selected scopes by default
				settings.Auth.ScopesFullSet.Keys.ToList().ForEach(scope => cfg.OAuth2Client.Scopes.Add(scope));
			});

		return app;
	}

	private static string GetAuthEndpoint(string endpointSuffix) => $"/connect/{endpointSuffix}";
}