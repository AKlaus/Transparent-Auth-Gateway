[![Build](https://github.com/AKlaus/Transparent-Auth-Gateway/actions/workflows/build.yml/badge.svg)](https://github.com/AKlaus/Transparent-Auth-Gateway/actions/workflows/build.yml)

Code samples for a series of articles about implementing Transparent Auth Gateway:
1. [Needs and means](https://alex-klaus.com/transparent-auth-gateway-1). Requirements for enterprise apps and available off-the-shelf solutions.
2. [Auth Flows](https://alex-klaus.com/transparent-auth-gateway-2). Relevant authentication/authorisation flows (OAuth2, OIDC) with sequence diagrams.
3. [Writing the code in C#](https://alex-klaus.com/transparent-auth-gateway-3). Comments to the code in this repo – a _Transparent Auth Gateway_ in .NET.
4. [Deploying to Azure](https://alex-klaus.com/transparent-auth-gateway-4). _App Registrations_ and Firewall settings (Azure _WAF_ / _Front Door_).

# Transparent Auth Gateway for Enterprise apps

A trusted authority for our enterprise application(s) that
- transparently (without additional user interaction) confirms the identity with the linked _Identity Provider_ (an _Azure AD_ tenant in this case), supporting SSO;
- conducts extra authentication checks (with a potential for own user management);
- issues an _access token_ with app-specific attributes (user’s roles/groups/etc.);
- is self-hosted without reliance on third-party services.

The code uses _Azure AD_ as the linked _Identity Provider_ (for the identity checks) and its own bespoke authorisation server. 

![Transparent Auth Gateway](./auth-gateway-enterprise-apps.png)

The implemented protocols:
- OpenID Connect ([OIDC](https://openid.net/connect/)).
- OAuth 2 [Authorisation Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) ([RFC 6749](https://www.rfc-editor.org/rfc/rfc6749#section-4.1)) with [Proof Key for Code Exchange](https://www.oauth.com/oauth2-servers/pkce/) ([RFC 7636](https://www.rfc-editor.org/rfc/rfc7636)).
- OAuth 2 [Client Credentials Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/client-credentials-flow).

# Code structure
There are 3 projects:

- [AzureADAuthClient](./AzureADAuthClient) – a quick way to ensure that _Azure AD_ authentication is configured. Uses Swagger UI to acquire a token and the standard `Microsoft.Identity` way to validate the token on WebAPI.
- [OpenIdDict.Server](./OpenIdDict.Server) – a bespoke _Transparent Auth Gateway_ to confirm the user's identity from the linked provider and authorise the user (issue own _access token_) according to the bespoke rules:
  - implements OAuth 2 flows to serve as the trusted authorization authority to other client apps:
    - [Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) with [PKCE](https://oauth.net/2/pkce/) for user authorization;
    - [Client Credentials Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/client-credentials-flow) for API integration; 
  - for users authorization, it perform authentication from the linked _Identity Provider_ (a specified tenant of Azure AD).
- [OpenIdDict.Client.Api](./OpenIdDict.Client.Api) – A Web API app that validates the _access token_ issued by the Auth Gateway (`OpenIdDict.Server`). Contains:
  - Swagger front-end to request the token and run HTTP requests;
  - test API end-points.

# How's it different?
The key differences:
- Issues its own _access token_ based on internal rules and confirmed user's identity from an Azure AD tenant.
- Requires no database.
- Has minimum code and "magical" behaviour from the packages.
