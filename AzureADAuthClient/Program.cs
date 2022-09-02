using AK.OAuthSamples.AzureADAuthClient.Configuration;
using AK.OAuthSamples.AzureADAuthClient.Routes;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local"))
	builder.Configuration.AddUserSecrets<AppSettings>();

var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
builder.Services.AddAndConfigureSwagger(settings)
				.AddAndConfigureAuthorisation(settings.AzureAd);

var app = builder.Build();

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local"))
	// Enable showing extra debug information in the console 
	IdentityModelEventSource.ShowPII = true; 

app .UseDeveloperExceptionPage()
	.UseHttpsRedirection()
	.ConfigureSwagger(settings)
	.UseAuthentication()
	.UseAuthorization();
	
app.MapTestRoutes();

app.Run();