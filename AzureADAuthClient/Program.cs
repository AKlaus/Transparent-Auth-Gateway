using AK.OAuthSamples.AzureADAuthClient.Configuration;
using AK.OAuthSamples.AzureADAuthClient.Routes;

using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

var isLocal = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local");
if (isLocal)
	builder.Configuration.AddUserSecrets<AppSettings>();

// Resolving the settings
var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
// Configuring the IoC/DI container
builder.Services.AddAndConfigureSwagger(settings)
				.AddAndConfigureAuthorisation(settings.AzureAd);

// Building the middleware pipeline
var app = builder.Build();

if (isLocal)
	app.UseDeveloperExceptionPage();

app	.UseHttpsRedirection();

if (isLocal)
	app.ConfigureSwagger(settings);	// Swagger is available in DEV only

app .UseAuthentication()
	.UseAuthorization();	// Required only if the project has secured end-points 
	
app.MapTestRoutes();

if (isLocal)
	// Enable showing extra debug information in the console. Must be added right before `Run()`, see https://stackoverflow.com/a/73956586/968003
	IdentityModelEventSource.ShowPII = true;

app.Run();