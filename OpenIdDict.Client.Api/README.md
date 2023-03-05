# Web API with OAuth 2 authorisation via a custom Auth Gateway

## 1. Purpose

A Web API app that validates the _access token_ issued by a custom Auth Gateway along with a Swagger front-end to request the token and run HTTP requests against test end-points.

The used protocol OAuth 2 [Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) with Proof Key for Code Exchange ([PKCE](https://oauth.net/2/pkce/)).

## 2. Run it
**Prerequisite**: a running Auth Gateway (see [OpenIdDict.Server](../OpenIdDict.Server) project).

1. Launch it.
2. Try the two available end-points. `/anonymous` must return a HTTP 200 code, while `/protected` gives a 401.
3. Pass the Swagger authentication.<br> It will pop up a new tab with the Auth Gateway authentication (that redirects further to the linked Identity Provider) and will close it on successful authentication.
4. Try `/protected` end-point and it must return a HTTP 200 code.

## 3. Implementation

The API has just two end-points:
- `/anonymous` that always returns HTTP code 200 on any request.
- `/protected` that requires user to authenticate and provide a Bearer token from the Auth Gateway (see [OpenIdDict.Server](../OpenIdDict.Server) project). Otherwise, it returns HTTP code 401 Unauthorized.

The authentication is handled by [NSwag](https://github.com/RicoSuter/NSwag):
1. The user gets redirected to the Auth Gateway's `/authorize` route.<br>
E.g. `https://localhost:5001/connect/authorize?response_type=code&client_id=TestApp&redirect_uri=https%3A%2F%2Flocalhost%3A5003%2Fswagger%2Foauth2-redirect.html&scope=openid&state={STATE}&realm=realm&code_challenge={CODE_CHALLENGE}&code_challenge_method=S256` 
2. If a relevant user identity cookie not found, 
   1. the user gets redirected further to the login page of the linked _Identity Provider_ (for Azure AD it's `https://login.microsoftonline.com/{TENANT}/oauth2/v2.0/authorize`).
   2. on successful authentication withing the tenant, the user gets redirected back to the Auth Gateway to continue the authentication/authorisation process.
3. On successful authentication/authorisation within the Auth Gateway the user gets redirected back to Swagger<br> `https://localhost:5001/swagger/oauth2-redirect.html?code={RECEIVED_CODE}&session_state={RANDOM_STATE}`
4. Then _NSwag_'s JavaScript exchanges the received _code_ to an _access token_ by running a `POST` request to `https://localhost:5001/connect/token`.
