using AK.OAuthSamples.OpenIdDict.Server.Configuration;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Abstractions;
using OpenIddict.Server;
// ReSharper disable ClassNeverInstantiated.Global

namespace AK.OAuthSamples.OpenIdDict.Server.Authorisation;

//
// The default implementation uses encrypted JWTs as Auth Codes with all the identity information. It (especially the JWT signature)
// blows out the size of the generated code string (may exceed 2,048 symbols) that gets returned in the Query String.
// Kestrel / Firewalls / browsers may prohibit requests with such long query strings.
// As a workaround, the code below replaces the 'auth code' with a 'reference token' (a GUID in this case) and stores the pair in memory cache.
// This approach is not a part of the OAuth2 spec (as it doesn't specify what the 'auth code' must look like) but a widely accepted practice.
// Here are some links: 
//		- Auth0 calls them “opaque” token: https://auth0.com/docs/secure/tokens/access-tokens#opaque-access-tokens
//		- IdentityServer4 is using ref tokens: https://identityserver4.readthedocs.io/en/latest/topics/reference_tokens.html
//		- LeastPrivilege: https://leastprivilege.com/2015/11/25/reference-tokens-and-introspection/
//


/// <summary>
///		Caches the <i>Authorization Code</i> in memory cache and replaces it with a reference that can be resolved later
/// </summary>
/// <remarks>
///		Requires degraded mode.
/// </remarks>
public sealed class CodeReferenceTokenStorageHandler : IOpenIddictServerHandler<OpenIddictServerEvents.GenerateTokenContext>
{
    private readonly IMemoryCache _memoryCache;

    public CodeReferenceTokenStorageHandler(IMemoryCache memoryCache)
        => _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

    /// <summary>
    /// Gets the default descriptor definition assigned to this handler.
    /// </summary>
    public static OpenIddictServerHandlerDescriptor Descriptor { get; }
        = OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.GenerateTokenContext>()
            .AddFilter<RequireDegradedModeEnabled>()
            .UseScopedHandler<CodeReferenceTokenStorageHandler>()
            .SetOrder(OpenIddictServerHandlers.Protection.GenerateIdentityModelToken.Descriptor.Order + 2_000)
            .SetType(OpenIddictServerHandlerType.BuiltIn)
            .Build();

    /// <inheritdoc/>
    public ValueTask HandleAsync(OpenIddictServerEvents.GenerateTokenContext context)
    {
		if (context is null)
		    throw new ArgumentNullException(nameof(context));

		if (context.TokenType == OpenIddictConstants.TokenTypeHints.AuthorizationCode)
		{
			var codeTokenReference = Base64UrlEncoder.Encode(Guid.NewGuid().ToByteArray());
			_memoryCache.Set(	codeTokenReference, 
								context.Token, 
								new MemoryCacheEntryOptions().SetSlidingExpiration(ServiceCollectionExtensions.AuthorizationCodeLifetime)
							);

			context.Token = codeTokenReference;
		}

		return ValueTask.CompletedTask;
    }
}

/// <summary>
///		Resolves an <i>Authorization Code</i> reference from memory cache and replaces it with the actual code
/// </summary>
/// <remarks>
///		Requires degraded mode.
/// </remarks>
public sealed class ValidateCodeReferenceTokenHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateTokenContext>
{
	private readonly IMemoryCache _memoryCache;

	public ValidateCodeReferenceTokenHandler(IMemoryCache memoryCache)
		=> _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	
    /// <summary>
    /// Gets the default descriptor definition assigned to this handler.
    /// </summary>
    public static OpenIddictServerHandlerDescriptor Descriptor { get; }
        = OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.ValidateTokenContext>()
            .AddFilter<RequireDegradedModeEnabled>()
            .UseScopedHandler<ValidateCodeReferenceTokenHandler>()
            .SetOrder(OpenIddictServerHandlers.Protection.ResolveTokenValidationParameters.Descriptor.Order + 2_000)
            .SetType(OpenIddictServerHandlerType.BuiltIn)
            .Build();

    public ValueTask HandleAsync(OpenIddictServerEvents.ValidateTokenContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (_memoryCache.TryGetValue(context.Token, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
	        context.Token = cachedToken;

        return ValueTask.CompletedTask;
    }
}

/// <summary>
///		A filter that excludes the associated handlers if the degraded mode is <b>enabled</b>.
/// </summary>
public sealed class RequireDegradedModeEnabled : IOpenIddictServerHandlerFilter<OpenIddictServerEvents.BaseContext>
{
	/// <inheritdoc/>
	public ValueTask<bool> IsActiveAsync(OpenIddictServerEvents.BaseContext context)
	{
		if (context is null)
			throw new ArgumentNullException(nameof(context));

		return new(context.Options.EnableDegradedMode);
	}
}