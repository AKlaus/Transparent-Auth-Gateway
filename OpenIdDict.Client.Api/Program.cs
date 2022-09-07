using AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;
using AK.OAuthSamples.OpenIdDict.Client.Api.Routes;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
builder.Services.AddAndConfigureSwagger(settings)
				.AddAndConfigureAuthorisation(settings.OAuth);

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