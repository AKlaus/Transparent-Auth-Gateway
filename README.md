Code samples for a series of articles about implementing Transparent Auth Gateway:
1. [Needs and means](https://alex-klaus.com/transparent-auth-gateway-1). Requirements and available off-the-shelf solutions.
2. [Auth Flows](https://alex-klaus.com/transparent-auth-gateway-2). Sequence diagrams of authentication/authorisation flows.
3. [Writing the code](https://alex-klaus.com/transparent-auth-gateway-3). Creating a _Transparent Auth Gateway_ in .NET.

# Transparent Auth Gateway for Enterprise apps

A trusted authority for our enterprise application(s) that
- transparently (without user interaction) confirms the identity from the single authentication authority, supporting SSO;
- issues a JWT or SAML token with app specific attributes (user’s roles/groups/etc.);
- is self-hosted, again.

The code uses _Azure AD_ as the linked _Identity Provider_ (for the identity checks) and an own bespoke authorisation server. 

![Transparent Auth Gateway](./auth-gateway-enterprise-apps.png)

The implemented protocols:
- OAuth 2 [Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) with Proof Key for Code Exchange ([PKCE](https://oauth.net/2/pkce/)).
- OpenID Connect ([OIDC](https://openid.net/connect/)).

# Code structure
There are 3 projects:

- [AzureADAuthClient](./AzureADAuthClient) – a quick way to ensure that _Azure AD_ authentication is configured. Uses Swagger UI to acquire a token and the standard `Microsoft.Identity` way to validate the token on WebAPI.
- [OpenIdDict.Server](./OpenIdDict.Server) – a bespoke _Transparent Auth Gateway_ that that implements OAuth 2 [Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) with [PKCE](https://oauth.net/2/pkce/) to serve other client apps as a trusted authority and perform authentication from a linked _Identity Provider_ (a specified tenant of Azure AD).
- [OpenIdDict.Client.Api](./OpenIdDict.Client.Api) – A Web API app that validates the _access token_ issued by a custom Auth Gateway along with a Swagger front-end to request the token and run HTTP requests against test end-points.

