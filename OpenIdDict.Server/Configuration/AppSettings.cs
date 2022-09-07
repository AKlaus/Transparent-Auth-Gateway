using OpenIddict.Abstractions;

namespace AK.OAuthSamples.OpenIdDict.Server.Configuration;

public class AppSettings
{
	/// <summary>
	///		Name of the application for Swagger UI
	/// </summary>
	public string AppName { get; private set; } = null!;
	
	/// <summary>
	///		Settings for the App Registration and Azure AD 
	/// </summary>
	public AzureCredentialsSettings AzureAd { get; private set; } = null!;

	/// <summary>
	///		OAuth Server settings 
	/// </summary>
	public AuthCredentialsSettings Auth { get; private set; } = null!;

	public class AuthCredentialsSettings
	{
		public string ClientId { get; private set; } = null!;
		/// <summary>
		///		The special scope used for the app
		/// </summary>
		public string Scope { get; private set; } = OpenIddictConstants.Scopes.Profile;
		/// <summary>
		///		A full set of all scopes including the mandatory ones that need to be included all the time (for events, swagger, etc.)
		/// </summary>
		public Dictionary<string, string> ScopesFullSet => new()
			{
				// The app scope
				{ Scope, "Access This API" }, 
				// Mandatory scopes
				{ OpenIddictConstants.Scopes.OpenId, "The default OIDC scope" }, 
				{ OpenIddictConstants.Scopes.OfflineAccess, "Scope to get a refresh token"}
			};
		
		/// <summary>
		///		The redirect URI on successful authentication 
		/// </summary>
		public string[] RedirectUris { get; private set; } = null!;
		/// <summary>
		///		The redirect URI on logout
		/// </summary>
		public string PostLogoutRedirectUri { get; private set; } = null!;
	}

	public class AzureCredentialsSettings
	{
		/// <summary>
		///		Name of your tenant (e.g. contoso.onmicrosoft.com) or its tenant ID (a GUID)
		/// </summary>
		public string Tenant { get; private set; } = null!;
		
		/// <summary>
		///		The Client Id (aka Application ID obtained from the "App registration" of the Azure Portal), e.g. ba74781c2-53c2-442a-97c2-3d60re42f403
		/// </summary>
		public string ClientId { get; private set; } = null!;
		
		/// <example>
		///		https://login.microsoftonline.com/
		/// </example>
		public string Instance { get; private set; } = null!;
		
		/// <summary>
		///		The scope on the API
		/// </summary>
		public string Scope { get; private set; } = null!;
	}
}