# Direct Azure AD authentication from Swagger

## 1. Purpose

A quick test of _Azure AD_ authentication with minimum code.

The used protocol OAuth 2 [Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) with Proof Key for Code Exchange ([PKCE](https://oauth.net/2/pkce/)).

## 2. Run it

**Prerequisite**: an _Azure AD_ tenant supporting authentication.

1. Configure `appsettings.json` (by setting parameters directly in the file or view [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)):
   - `AzureAd:Tenant` – the name of your tenant (e.g. _contoso.onmicrosoft.com_) or its tenant ID (a GUID). Sometime it's referred as the _issuer_<br>The parameter is used in forming a set of HTTP endpoints for the _Identity Provider_ (Azure AD in our case). E.g. `https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/authorize`.
   - `AzureAd:ClientId` – the _Application (client) ID_ (a GUID) of the _App Registration_.
   - `AzureAd:Scope` – The requested scope (also called [delegated permission](https://learn.microsoft.com/en-au/azure/active-directory/develop/permissions-consent-overview)), for the client apps to obtain an access token. Note that all APIs must publish a minimum of one scope and this app is using just one for simplicity.
2. Launch it.
3. Try the two available end-points. `/anonymous` must return a HTTP 200 code, while `/protected` gives a 401.  
4. Pass the Swagger authentication.<br> It will pop up a new tab with the Azure AD login page and will close it on successful authentication.
5. Try `/protected` end-point and it must return a HTTP 200 code.

Check out [this instruction](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/4-WebApp-your-API/4-1-MyOrg#step-3-register-the-sample-applications-in-your-tenant) for registering an _App Registration_ (manually or by running a PowerShell script) and configuring the scopes.  

## 3. Implementation

The API has just two end-points:
- `/anonymous` that returns HTTP code 200 on any request with or without authentications and the bearer token.
- `/protected` that requires user to authenticate and provide a Bearer token on request. Otherwise, it returns HTTP code 401 Unauthorized. 

The authentication is handled by [NSwag](https://github.com/RicoSuter/NSwag):
1. The user gets redirected to <br>`https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/authorize?response_type=code&client_id={CLIENT}&redirect_uri=https%3A%2F%2Flocalhost%3A5001%2Fswagger%2Foauth2-redirect.html&scope={SCOPE}&state={RANDOM_STATE}%3D&realm=realm&code_challenge={CODE_CHALLENGE}&code_challenge_method=S256`
2. If a relevant user identity cookie not found, the user gets redirected to the login page of the tenant.
3. On successful authentication withing the tenant, the user gets redirected back to <br> `https://localhost:5001/swagger/oauth2-redirect.html?code={RECEIVED_CODE}&session_state={RANDOM_STATE}`
4. Then _NSwag_'s JavaScript exchanges the received _code_ to an _access token_ by<br>  
```
POST https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/token
grant_type: authorization_code
code: {RECEIVED_CODE}
client_id: {CLIENT}
redirect_uri: https://localhost:5001/swagger/oauth2-redirect.html
code_verifier: {...}
```
