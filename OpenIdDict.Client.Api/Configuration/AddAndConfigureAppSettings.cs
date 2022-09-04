using Microsoft.Extensions.Options;

namespace AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register Global Settings.
	///		Note: it must be executed as the 1st step in building services
	/// </summary>
	public static AppSettings AddAndConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<AppSettings>()
				.Bind(configuration, c => c.BindNonPublicProperties = true)
				.ValidateDataAnnotations()
				.ValidateOnStart();

		services.AddSingleton(r => r.GetRequiredService<IOptions<AppSettings>>().Value);

		// Makes IServiceProvider available in the container.
		// Note that this step may resolve in duplicates of registered objects. It's safe to apply as the 1st step in building services
		var resolver = services.BuildServiceProvider();
		
		return resolver.GetRequiredService<AppSettings>();
	}
}