# Azure AD authentication with using `id_token` only

## 1. Purpose

A quick test of _Azure AD_ authentication with `id_token` only implementing minimum code. This project is for testing purposes to proceed further to Auth Gateway development. See [this article](https://alex-klaus.com/transparent-auth-gateway-3/) for a detailed explanation. 

The used protocol is OAuth 2 [Implicit Grant](https://oauth.net/2/grant-types/implicit/) Flow ([RFC 6749 sec 1.3.2](https://tools.ietf.org/html/rfc6749#section-1.3.2)).

## 2. Run it

**Prerequisite**: an _Azure AD_ tenant supporting authentication.

1. Configure `appsettings.json` (by setting parameters directly in the file or view [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)):
   - `AzureAd:Tenant` – the name of your tenant (e.g. _contoso.onmicrosoft.com_) or its tenant ID (a GUID). Sometime it's referred as the _issuer_<br>The parameter is used in forming a set of HTTP endpoints for the _Identity Provider_ (Azure AD in our case). E.g. `https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/authorize`.
   - `AzureAd:ClientId` – the _Application (client) ID_ (a GUID) of the _App Registration_.
   - `AzureAd:Scope` – [OPTIONAL] The requested scope (also called [delegated permission](https://learn.microsoft.com/en-au/azure/active-directory/develop/permissions-consent-overview)), for the client apps to obtain an access token. Note that Azure AD demands all APIs must publish a minimum of one scope and this app is using just one for simplicity.
2. Launch it.
3. Try the two available end-points. `/anonymous` must return a HTTP 200 code, while `/protected` gives a 401.  
4. Pass the Swagger authentication.<br> It will pop up a new tab with the Azure AD login page and will close it on successful authentication.
5. Try `/protected` end-point and it must return a HTTP 200 code.

Check out [this instruction](https://alex-klaus.com/transparent-auth-gateway-4/) for registering an _App Registration_ (manually or by running a PowerShell script) and configuring the scopes.  

## 3. Implementation

The API has just two end-points:
- `/anonymous` that returns HTTP code 200 on any request with or without authentications and the bearer token.
- `/protected` that requires user to authenticate and provide a Bearer token on request. Otherwise, it returns HTTP code 401 Unauthorized. 

The authentication client is implemented by [NSwag](https://github.com/RicoSuter/NSwag):
1. The user gets redirected to <br>`https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/authorize?nonce=SWAGGER&response_type=id_token&client_id={CLIENT}&redirect_uri=https%3A%2F%2Flocalhost%3A5003%2Fswagger%2Foauth2-redirect.html&scope={SCOPES}&state={RANDOM_STATE}&realm=realm`
2. If a relevant user identity cookie not found, the user gets redirected to the login page of the Azure AD tenant.
3. On successful authentication withing the tenant, the user gets redirected back to <br> `https://localhost:5003/swagger/oauth2-redirect.html#id_token=={RECEIVED_ID_TOKEN}&session_state={RANDOM_STATE}`
