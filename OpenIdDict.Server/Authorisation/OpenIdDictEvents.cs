using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;

using OpenIddict.Abstractions;
using OpenIddict.Server;

using AK.OAuthSamples.OpenIdDict.Server.Configuration;

namespace AK.OAuthSamples.OpenIdDict.Server.Authorisation;

internal static class OpenIdDictEvents
{
	/// <summary>
	///		Validation of `/authorize` requests
	/// </summary>
	internal static Func<OpenIddictServerEvents.ValidateAuthorizationRequestContext, ValueTask> ValidateAuthorizationRequestFunc(AppSettings.AuthCredentialsSettings authSettings) => 
		validateAuthorizationRequestContext =>
		{
			if (!string.Equals(validateAuthorizationRequestContext.ClientId, authSettings.ClientId, StringComparison.OrdinalIgnoreCase))
			{
				validateAuthorizationRequestContext.Reject(
					error: OpenIddictConstants.Errors.InvalidClient,
					description: "The specified 'client_id' doesn't match a registered application.");
				return default;
			}
			if (authSettings.RedirectUris?.Any() != true)
			{
				validateAuthorizationRequestContext.Reject(
					error: OpenIddictConstants.Errors.InvalidRequestUri,
					description: "Server has no configured allowed 'redirect_uri'.");
				return default;
			}
			if (!authSettings.RedirectUris.Contains(validateAuthorizationRequestContext.RedirectUri, StringComparer.OrdinalIgnoreCase))
			{
				validateAuthorizationRequestContext.Reject(
					error: OpenIddictConstants.Errors.InvalidRequestUri,
					description: "The specified 'redirect_uri' is not valid for this client application.");
				return default;
			}
			return default;
		};
	
	/// <summary>
	///		Validation of `/token` requests
	/// </summary>	
	internal static Func<OpenIddictServerEvents.ValidateTokenRequestContext, ValueTask> ValidateTokenRequestFunc(AppSettings.AuthCredentialsSettings authSettings) => 
		validateTokenRequestContext =>
		{
			if (validateTokenRequestContext.Request.IsAuthorizationCodeFlow()	// Auth Code Flows must provide a predefined client_id 
			    && !string.Equals(validateTokenRequestContext.ClientId, authSettings.ClientId, StringComparison.OrdinalIgnoreCase))
			{
				validateTokenRequestContext.Reject(
					error: OpenIddictConstants.Errors.InvalidClient,
					description: "The specified 'client_id' doesn't match a registered application.");
				return default;
			}
			// No client secret validation, as this project is used by a public client application
			return default;
		};

	/// <summary>
	///		Handling redirects to `/authorize` route for the Authorization Code Flow
	/// </summary>
	internal static Func<OpenIddictServerEvents.HandleAuthorizationRequestContext, ValueTask> HandleAuthorizationRequest(AppSettings.AuthCredentialsSettings authSettings) => 
		async context =>
		{
			var request = context.Transaction.GetHttpRequest() ??
			              throw new InvalidOperationException("The HTTP request cannot be retrieved.");
			
			// Confirm the Authorization Code Flow
			var openIdDictRequest = request.HttpContext.GetOpenIddictServerRequest();
			if (openIdDictRequest?.IsAuthorizationCodeFlow() != true)
				return;

			// Retrieve the user principal stored in the user profile cookie.
			var authResult = await request.HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
			// If the principal cannot be retrieved, this indicates that the user is not logged in.
			if (authResult?.Succeeded != true || authResult?.Principal == null) 
			{
				// Auth challenge is triggered to redirect the user to the provider's authentication end-point 
				var properties = new AuthenticationProperties { Items = { ["LoginProvider"] = "Microsoft" } };
				await request.HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);
				context.HandleRequest();
				return;
			}

			// Now, identity of the user confirmed by the linked OIDC Provider.
			string email = ResolveAzureAdEmailClaim(authResult.Principal);
			string name = ResolveAzureAdUserNameClaim(authResult.Principal);
			
			/*
			 * Here's a place for app-specific authorisation and resolving app-related user's claims.
			 * If the app's authorisation fails then you can return a 403 page by
			 *		request.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
			 *		await request.HttpContext.Response.WriteAsync('Not registered');
			 *		context.HandleRequest();
			 *		return;
			 */ 
			
			// Form new claims
			var claimsIdentity = CreateClaimsIdentity(email, name);

			// Attach the principal to the authorization context, so that an OpenID Connect response
			// with an authorization code can be generated by the OpenIddict server services.
			context.Principal = new ClaimsPrincipal(claimsIdentity)
									// Re-attach the requested scopes adding the mandatory ones, so downstream handlers don't reject 'unsupported' scopes or not issue a 'refresh_token' if 'offline_access' scope is absent
									.SetScopes(openIdDictRequest.GetScopes().Union(authSettings.ScopesFullSet.Keys));
		};

	/// <summary>
	///		Handling requests to `/token` route for the Client Credentials Flow only
	/// </summary>
	internal static Func<OpenIddictServerEvents.HandleTokenRequestContext, ValueTask> HandleClientCredentialsTokenRequest(AppSettings.AuthCredentialsSettings authSettings) => 
		context =>
		{
			var httpRequest = context.Transaction.GetHttpRequest() ??
							throw new InvalidOperationException("The HTTP request cannot be retrieved.");
			
			// For non-Client Credentials flows we use the default processing (for Auth Code flow it lands in `HandleAuthorizationRequest`)
			// Note, the `/token` point can be called for the the Authorization Code Flow too.
			var openIdDictRequest = httpRequest.HttpContext.GetOpenIddictServerRequest();
			if (openIdDictRequest?.IsClientCredentialsGrantType() != true)
				return default;
			
			// Resolve the user by the client_id and client_secret.
			(string email, string name) = ResolveUserByClientCredentials(openIdDictRequest.ClientId, openIdDictRequest.ClientSecret, authSettings);
			if ((email, name) == default)
			{
				/*
				 * If the app's authorisation fails then you can return a 403 page by
				 *		request.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
				 *		await request.HttpContext.Response.WriteAsync('Not registered');
				 *		context.HandleRequest();
				 *		return default;
				 */ 
			}

			var claimsIdentity = CreateClaimsIdentity(email, name, TimeSpan.FromHours(6));
				
			// Attach the principal to the authorization context, so that an OpenID Connect response
			// with an authorization code can be generated by the OpenIddict server services.
			context.Principal = new ClaimsPrincipal(claimsIdentity)
									.SetScopes(openIdDictRequest.GetScopes());
			
			return default;
		};

	/// <summary>
	///		Form the claims, the identity type and the expiration period for the security principal
	/// </summary>
	private static ClaimsIdentity CreateClaimsIdentity(string email, string name, TimeSpan? accessTokenLifetime = null)
	{
		// Form new claims
		var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType /* sets it to 'Federated Authentication' */);
		identity.SetClaim(OpenIddictConstants.Claims.Subject /* unique identifier of the user */, email /* or any unique mandatory identifier, see RFC-7519, 4.1.2 */);
		identity.SetClaim(OpenIddictConstants.Claims.Email, email);
		identity.SetClaim(OpenIddictConstants.Claims.Name, name);

		/*
		 * Here's a place for app-specific authorisation and resolving app-related user's claims.
		 */

		// Set the destination for the added claims to 'Access Token', as they're authorisation-related attributes.
		// Another plus, the API controllers can retrieve them from the ClaimsPrincipal instance.
		identity.SetDestinations(_ => new[] { OpenIddictConstants.Destinations.AccessToken });

		// The default token's lifetime is 1 hour, but you can override here
		if (accessTokenLifetime.HasValue)
			identity.SetAccessTokenLifetime(accessTokenLifetime);

		return identity;
	}

	/// <summary>
	///		Resolving mandatory email from a relevant mapped claim 
	/// </summary>
	/// <exception cref="Exception"> Email can't be resolved </exception>
	private static string ResolveAzureAdEmailClaim(ClaimsPrincipal claims) => claims.GetClaim(OpenIddictConstants.Claims.PreferredUsername) ?? throw new Exception("Email can't be resolved"); 
	/// <summary>
	///		Resolve an optional name from the claims
	/// </summary>
	private static string ResolveAzureAdUserNameClaim(ClaimsPrincipal claims) => claims.GetClaim(OpenIddictConstants.Claims.Name) ?? string.Empty;

	/// <summary>
	///		A dummy check for correct credentials
	/// </summary>
	private static (string email, string name) ResolveUserByClientCredentials(string? clientId, string? clientSecret, AppSettings.AuthCredentialsSettings authSettings)
	{
		if (clientId == authSettings.ClientId)
			return ("test@email", "Test Name");
		return default;
	}
}