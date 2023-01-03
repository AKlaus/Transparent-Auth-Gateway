using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;

internal static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddAndConfigureAuthorisation(this IServiceCollection services, AppSettings.AuthCredentialsSettings settings)
	{
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(o =>
				{
					o.Authority = settings.Authority;
					o.TokenValidationParameters.ValidateAudience = false;
					// NOTE: To prevent a double round trip to '/.well-known/openid-configuration' and then '/.well-known/jwks', we can set the public key here
					// (https://stackoverflow.com/a/59847808/968003)
					// o.TokenValidationParameters.IssuerSigningKey = GetKey();
				});

		services.AddAuthorization();
		
		return services;
	}
}