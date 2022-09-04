using AK.OAuthSamples.OpenIdDict.Client.Api.Configuration;
using AK.OAuthSamples.OpenIdDict.Client.Api.Routes;

var builder = WebApplication.CreateBuilder(args);

var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
builder.Services.AddAndConfigureSwagger(settings)
				.AddAndConfigureAuthorisation(settings.OAuth);

var app = builder.Build();
app .UseDeveloperExceptionPage()
	.UseHttpsRedirection()
	.ConfigureSwagger(settings)
	.UseAuthentication()
	.UseAuthorization();
	
app.MapTestRoutes();

app.Run();