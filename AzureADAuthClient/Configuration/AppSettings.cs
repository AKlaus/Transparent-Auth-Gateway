namespace AK.OAuthSamples.AzureADAuthClient.Configuration;

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

	// ReSharper disable once ClassNeverInstantiated.Global
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
		
		/// <summary>
		/// The authority (Instance+Tenant).
		/// </summary>
		public string Authority => Instance + Tenant;
	}
}