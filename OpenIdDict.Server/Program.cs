using AK.OAuthSamples.OpenIdDict.Server.Configuration;
using AK.OAuthSamples.OpenIdDict.Server.Routes;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local"))
	builder.Configuration.AddUserSecrets<AppSettings>();

var settings = builder.Services.AddAndConfigureAppSettings(builder.Configuration);
	
builder.Services.AddAndConfigureSwagger(settings)
				.AddCors()
				.AddAndConfigureAuthorisation(settings);

var app = builder.Build();

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local"))
	// Enable showing extra debug information in the console 
	IdentityModelEventSource.ShowPII = true; 
	
app .UseDeveloperExceptionPage()
	.UseHttpsRedirection()
	.ConfigureSwagger(settings)
	.UseCors (policyBuilder => 
			  policyBuilder .AllowAnyOrigin()
							.AllowAnyMethod()
							.AllowAnyHeader())
	.Use(async (context, next) =>
		{
			Console.WriteLine(context.Request.GetDisplayUrl());
			await next.Invoke();
		})
	.UseAuthentication()
	.UseAuthorization();

app.MapTestRoutes();

app.Run();
