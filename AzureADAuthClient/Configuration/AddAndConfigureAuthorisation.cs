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
					/*	Uncomment for trouble-shooting. See https://stackoverflow.com/a/75624357/968003
						jwtOptions.Events = new JwtBearerEvents
						{
							// If a JWT token gets rejected as invalid, set a breakpoint on the context of 'OnChallenge' event
							OnChallenge = context => Task.CompletedTask,
							// Other events are here just in case
							OnForbidden = context => Task.CompletedTask,
							OnAuthenticationFailed = context => Task.CompletedTask,
							OnMessageReceived = context => Task.CompletedTask,
							OnTokenValidated = context => Task.CompletedTask,
						};*/
					}, 
					idOptions =>
					{
						idOptions.Instance = settings.Instance;
						idOptions.ClientId = settings.ClientId;
						idOptions.ClientSecret = String.Empty;	// Not applicable to implicit flow
						idOptions.TenantId = settings.Tenant;
						idOptions.Scope.Add(settings.Scope);	// Can be omitted here, as we deal with `id_token` only where AzureAD doesn't specify scopes
						idOptions.AllowWebApiToBeAuthorizedByACL = true;	// Allow authorisations of tokens without roles and scopes 
					});
		return services;
	}
}