# Transparent Auth Gateway providing an OIDC authentication  from a linked Azure AD tenant and implementing OAuth 2 authorisation for client apps

## 1. Purpose

A custom build Identity Server that implements OAuth 2 [Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) with [PKCE](https://oauth.net/2/pkce/) to serve other client apps as a trusted authority and perform authentication from a linked _Identity Provider_ (a specified tenant of Azure AD).

## 2. Run it

**Prerequisite**: an _Azure AD_ tenant supporting authentication.

1. Configure `appsettings.json` (by setting parameters directly in the file or view [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)):
   - `AzureAd:Tenant` – the name of your tenant (e.g. _contoso.onmicrosoft.com_) or its tenant ID (a GUID). Sometime it's referred as the _issuer_<br>The parameter is used in forming a set of HTTP endpoints for the _Identity Provider_ (Azure AD in our case). E.g. `https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/authorize`.
   - `AzureAd:ClientId` – the _Application (client) ID_ (a GUID) of the _App Registration_.
   - `AzureAd:Scope` – The requested scope (also called [delegated permission](https://learn.microsoft.com/en-au/azure/active-directory/develop/permissions-consent-overview)), for the client apps to obtain an access token. Note that all APIs must publish a minimum of one scope and this app is using just one for simplicity.
2. Launch it.
3. Try the two available end-points. `/anonymous` must return a HTTP 200 code, while `/protected` gives a 401.
4. Pass the Swagger authentication.<br> It will pop up a new tab with a bunch of redirects that brings the user to the Azure AD login page and will close it on successful authentication.
5. Try `/protected` end-point and it must return a HTTP 200 code.

## 3. Implementation

The API has just two end-points:
- `/anonymous` that always returns HTTP code 200 on any request.
- `/protected` that requires user to authenticate and provide a self-issued Bearer token. Otherwise, it returns HTTP code 401 Unauthorized.

Handling OAuth 2  [Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) with [PKCE](https://oauth.net/2/pkce/) is implemented with using [OpenIdDict](https://github.com/openiddict/openiddict-core) NuGet package with the key implementation is in `HandleAuthorizationRequestContext` handler of its 'degraded mode' (see [this article](https://kevinchalet.com/2020/02/18/creating-an-openid-connect-server-proxy-with-openiddict-3-0-s-degraded-mode/) from the author of the library on detailed implementation). 

The authentication client is implemented by [NSwag](https://github.com/RicoSuter/NSwag):
1. The user gets redirected to `/authorize` route.<br>
   E.g. `/connect/authorize?response_type=code&client_id=TestApp&redirect_uri=https%3A%2F%2Flocalhost%3A5003%2Fswagger%2Foauth2-redirect.html&scope=openid&state={STATE}&realm=realm&code_challenge={CODE_CHALLENGE}&code_challenge_method=S256`
2. If a relevant user identity cookie not found,
   1. the user gets redirected further to the login page of the linked _Identity Provider_ (for Azure AD it's `https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/authorize`).
   2. on successful authentication withing the tenant, the user gets redirected back to the Auth Gateway to continue the authentication/authorisation process.
3. On successful authentication/authorisation, the user gets redirected back to Swagger<br> `/swagger/oauth2-redirect.html?code={CODE}&session_state={RANDOM_STATE}`,<br>where `CODE` is a reference token to the auth code stored in memory cache on the server.
4. Then _NSwag_'s JavaScript exchanges the received _code_ to an _access token_ by running a `POST` request to `/token`.
