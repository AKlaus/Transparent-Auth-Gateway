using Microsoft.AspNetCore.Authorization;

namespace AK.OAuthSamples.AzureADAuthClient.Routes;

internal static class RoutesExtension 
{
	/// <summary>
	///		Map test routes
	/// </summary>
	public static void MapTestRoutes(this IEndpointRouteBuilder app)
	{
		app.MapGet(
			"/anonymous", 
			[AllowAnonymous] () => "Anonymous access succeeded")
			.WithTags("Test API");

		app.MapGet(
			"/protected",
			[Authorize] () => "Authorised access succeeded")
			.WithTags("Test API");
	}
}
