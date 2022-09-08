using AK.OAuthSamples.OpenIdDict.Server.Configuration;
using AK.OAuthSamples.OpenIdDict.Server.Routes;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

var isLocal = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local");

if (isLocal)
{
	builder.Configuration.AddUserSecrets<AppSettings>();
	// Enable showing extra debug information in the console 
	IdentityModelEventSource.ShowPII = true; }

var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
builder.Services.AddAndConfigureSwagger(settings)
				.AddCors()
				.AddAndConfigureAuthorisation(settings);

var app = builder.Build();
	
app .UseDeveloperExceptionPage()
	.UseHttpsRedirection()
	.ConfigureSwagger(settings)
	.UseCors (policyBuilder => 
			  policyBuilder .AllowAnyOrigin()
							.AllowAnyMethod()
							.AllowAnyHeader())
	.Use(async (context, next) =>
		{
			if (isLocal)
				Console.WriteLine(context.Request.GetDisplayUrl());
			await next.Invoke();
		})
	.UseAuthentication()
	.UseAuthorization();

app.MapTestRoutes();

app.Run();

