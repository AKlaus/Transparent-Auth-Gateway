using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace AK.OAuthSamples.AzureADAuthClient.Configuration;

internal static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddAndConfigureAuthorisation(this IServiceCollection services, AppSettings.AzureCredentialsSettings settings)
	{
		services.AddAuthorization()
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				// adds the Microsoft identity platform authorization to web API and configures validation of access tokens
				.AddMicrosoftIdentityWebApi(
					jwtOptions =>
					{
						jwtOptions.Authority = settings.Authority;
						jwtOptions.Audience = settings.ClientId;
					}, 
					idOptions =>
					{
						idOptions.Instance = settings.Instance;
						idOptions.ClientId = settings.ClientId;
						idOptions.ClientSecret = String.Empty;
						idOptions.TenantId = settings.Tenant;
						idOptions.Scope.Add(settings.Scope);
					});
		return services;
	}
}