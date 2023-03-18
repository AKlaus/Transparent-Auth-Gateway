using AK.OAuthSamples.OpenIdDict.Server.Configuration;
using AK.OAuthSamples.OpenIdDict.Server.Routes;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

var isLocal = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local");

if (isLocal)
	builder.Configuration.AddUserSecrets<AppSettings>();

// Resolving the settings
var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
// Configuring the IoC/DI container
builder.Services.AddAndConfigureSwagger(settings)
				.AddCors()
				.AddAndConfigureAuthorisation(settings);

// Building the middleware pipeline
var app = builder.Build();

if (isLocal)
	app.UseDeveloperExceptionPage();

app	.UseHttpsRedirection();

if (isLocal)
	app.ConfigureSwagger(settings);	// Swagger is available in DEV only

app	.UseCors (policyBuilder => 
			  policyBuilder .AllowAnyOrigin()
							.AllowAnyMethod()
							.AllowAnyHeader())
	.Use(async (context, next) =>
		{	// Here's a convenient debugging hack
			if (isLocal)
				Console.WriteLine(context.Request.GetDisplayUrl());
			await next.Invoke();
		})
	.UseAuthentication()
	.UseAuthorization();	// Note 1: 'Authorization' is required only if the project has secured end-points
							// Note 2: the Authorization middleware must be register AFTER the Authentication middleware
app.MapTestRoutes();

if (isLocal)
	// Enable showing extra debug information in the console. Must be added right before `Run()`, see https://stackoverflow.com/a/73956586/968003
	// Includes potential PII (personally identifiable information) in exceptions in order to be in compliance with GDPR
	IdentityModelEventSource.ShowPII = true;

app.Run();