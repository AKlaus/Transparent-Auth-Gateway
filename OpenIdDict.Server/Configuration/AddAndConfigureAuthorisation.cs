using IdentityModel;
using Microsoft.Identity.Web;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;

namespace AK.OAuthSamples.OpenIdDict.Server.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register the auth engine
	/// </summary>
	/// <remarks>
	///		OAuth 2.0 uses two endpoints: `/authorize` and `/oauth/token` (https://auth0.com/docs/authenticate/protocols/oauth).
	///		Their meaning for the Authorization Code Flow:
	///			`/authorize` – issues an authorization code (redirects to Azure AD) 
	///			`/token` – exchanges the authorization code for an access token (how does it get used for refreshing the token?)
	/// </remarks>
	public static IServiceCollection AddAndConfigureAuthorisation(this IServiceCollection services, AppSettings settings)
	{
		// Register the OpenIddict services
		services.AddOpenIddict()
				// Register the OpenIddict server components.
				.AddServer(options =>
				{
					// Enable the authorization and token endpoints
					options 
							.SetTokenEndpointUris("/connect/token")
							.SetAuthorizationEndpointUris("/connect/authorize")
							//.SetVerificationEndpointUris("/connect/verify")
							.AddEventHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>(builder => builder.UseInlineHandler(OpenIdDictEvents.ValidateAuthorizationRequestFunc(settings.Auth)))
							.AddEventHandler<OpenIddictServerEvents.ValidateTokenRequestContext>(builder => builder.UseInlineHandler(OpenIdDictEvents.ValidateTokenRequestFunc(settings.Auth)))
							.AddEventHandler<OpenIddictServerEvents.HandleAuthorizationRequestContext>(builder => builder.UseInlineHandler(OpenIdDictEvents.HandleAuthorizationRequest))
					// Enable the Authorization Code Flow.
							.AllowAuthorizationCodeFlow()
							.RequireProofKeyForCodeExchange()
							.AllowRefreshTokenFlow()

					// Register the signing and encryption credentials
					// TODO: Ephemeral keys are used for development only and need to be replaced with a generated certificate  
							//.AddEphemeralEncryptionKey()
							//.AddEphemeralSigningKey()
							.AddDevelopmentEncryptionCertificate()
							.AddDevelopmentSigningCertificate()
							.DisableAccessTokenEncryption();

					// Register scopes (permissions)
					options.RegisterScopes(OidcConstants.StandardScopes.OpenId, settings.Auth.Scope);

					// Need Degraded Mode to use bare-bones of OpenIdDict
					options.EnableDegradedMode()
					// Register the ASP.NET Core host and configure the ASP.NET Core-specific options.	
							.UseAspNetCore();
				})
				// Register the OpenIddict validation components.
				.AddValidation(options =>
				{
					// Import the configuration from the local OpenIddict server instance.
					options.UseLocalServer();
					// Register the ASP.NET Core host.
					options.UseAspNetCore();
				});
		
		services.AddAuthorization()
				.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
				.AddMicrosoftIdentityWebApp(options =>
				{
					options.Instance = settings.AzureAd.Instance;
					options.TenantId = settings.AzureAd.Tenant;
					options.ClientId = settings.AzureAd.ClientId;
					options.Scope.Add(OidcConstants.StandardScopes.OpenId);
					options.Scope.Add(OidcConstants.StandardScopes.Profile);
					options.Scope.Add(settings.AzureAd.Scope);
				});
		return services;
	}
}