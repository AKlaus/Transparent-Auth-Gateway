using OpenIddict.Validation.AspNetCore;

namespace AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;

internal static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddAndConfigureAuthorisation(this IServiceCollection services, AppSettings.AuthCredentialsSettings settings)
	{
		services.AddOpenIddict()
				.AddValidation(options =>
				{
					// Note: the validation handler uses OpenID Connect discovery
					// to retrieve the address of the introspection endpoint.
					options.SetIssuer(settings.Authority);
					//options.AddAudiences("resource_server_1");

					// Configure the validation handler to use introspection and register the client
					// credentials used when communicating with the remote introspection endpoint.
					//options.UseIntrospection();
							//.SetClientId(settings.ClientId)
							// Don't set Client Secret as it's a public client, see more https://github.com/openiddict/openiddict-core/issues/701

					// Register the System.Net.Http integration.
					options.UseSystemNetHttp();

					// Register the ASP.NET Core host.
					options.UseAspNetCore();
				});
		services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

		services.AddAuthorization();
		
		return services;
	}
}