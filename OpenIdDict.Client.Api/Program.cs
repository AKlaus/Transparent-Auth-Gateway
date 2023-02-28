using AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;
using AK.OAuthSamples.OpenIdDict.Client.Api.Routes;

using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

var isLocal = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local");

if (isLocal)
{
	builder.Configuration.AddUserSecrets<AppSettings>();
	// Enable showing extra debug information in the console 
	IdentityModelEventSource.ShowPII = true;
}

// Resolving the settings
var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
// Configuring the IoC/DI container
builder.Services.AddAndConfigureSwagger(settings)
				.AddAndConfigureAuthorisation(settings.OAuth);

// Building the middleware pipeline
var app = builder.Build();

if (isLocal)
	app.UseDeveloperExceptionPage();

app	.UseHttpsRedirection();

if (isLocal)
	app.ConfigureSwagger(settings);	// Swagger is available in DEV only

app .UseAuthentication()
	.UseAuthorization();
	
app.MapTestRoutes();

app.Run();