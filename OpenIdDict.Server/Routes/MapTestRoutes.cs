using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;

namespace AK.IdentityServerSample.IdentityServer.Routes;

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
			[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)] () => "Authorised access succeeded")
			.WithTags("Test API");
	}
}
