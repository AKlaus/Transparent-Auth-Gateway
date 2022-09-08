namespace AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;

public class AppSettings
{
	/// <summary>
	///		Name of the application for Swagger UI
	/// </summary>
	public string AppName { get; private set; } = null!;
	
	/// <summary>
	///		OAuth Server settings 
	/// </summary>
	public AuthCredentialsSettings OAuth { get; private set; } = null!;

	public class AuthCredentialsSettings
	{
		public string ClientId { get; private set; } = null!;
		/// <summary>
		///		The scopes (e.g. 'profile')
		/// </summary>
		public string Scope { get; private set; } = "profile";
		
		/// <summary>
		///		The authority (URL of the Auth Server).
		/// </summary>
		public string Authority { get; private set; } = null!;
	}
}