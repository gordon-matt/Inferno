# Security Documentation

## Authentication & Authorization

### Overview

Inferno ships with two surface areas that authenticate:

1. **InfernoCMS** – the Blazor Server administration site. It uses ASP.NET Core Identity with cookie authentication and talks to the API in-process using JWTs minted locally by `ITokenService`.
2. **InfernoCMS.Api** – an OData/REST API protected by JWT bearer authentication. It exposes a minimal `/auth` surface so external applications can authenticate and call the API.

### Public API Authentication Endpoints

All `/auth/*` endpoints are rate limited (10 requests / minute / client IP) to blunt brute-force attacks.

#### `POST /auth/login`

Exchanges client + user credentials for a JWT bearer token.

Request body:

```json
{
  "apiKey": "<client api key>",
  "username": "<user name>",
  "password": "<user password>"
}
```

Behaviour:

- Validates the API client using a constant-time comparison against `ApiKey` in configuration.
- Authenticates the user via ASP.NET Core Identity with failed-login lockout enabled.
- Returns the same `Invalid credentials` error for unknown usernames and wrong passwords to prevent username enumeration.
- Returns a `TokenResponse` (`token`, `tokenType`, `userId`, `username`, `email`, `expiresAt`) on success.

#### `POST /auth/refresh`

Refreshes an existing JWT. Critically, the caller must present their previous (possibly expired) JWT – an API key alone is **not** sufficient, otherwise anyone with the API key could mint tokens for any user.

Request body:

```json
{
  "apiKey": "<client api key>",
  "token": "<previous jwt>"
}
```

Behaviour:

- Validates the provided token's signature, issuer and audience but **ignores lifetime** so expired tokens can still be exchanged.
- Rejects refresh if the user no longer exists, is currently locked out, or if the user's security stamp has rotated (e.g. password change, sign-out-everywhere).

#### `GET /auth/me`

Returns information about the caller identified by the bearer token. Useful for clients to validate their token and to discover the user's id, email and roles.

### Token Details

Tokens are HS256-signed JWTs issued by `InfernoCMS.Identity.Services.TokenService` with:

- `nameid` – user id
- `unique_name` – user name
- `email`, `role` claims
- `AspNet.Identity.SecurityStamp` – the user's security stamp (lets us invalidate old tokens on password change)
- `jti` – unique token id

Default lifetime is **120 minutes**. The matching bearer validator in `AddInfernoJwtBearer` uses a tight `ClockSkew` of 30 seconds and emits a `Token-Expired: true` response header when rejection is due to expiry, so clients can automatically refresh.

### Internal Token Flow (CMS → API)

The Blazor admin UI uses `RadzenODataService<TEntity, TKey>` to call the API. It mints tokens locally via `ITokenService` rather than calling `/auth/login`, which avoids a round-trip but means:

- Tokens are cached per user in a `ConcurrentDictionary` (NOT per service instance). The `IRadzenODataService<>` implementations are registered as singletons, so a per-instance cache would leak the first user's token to every other user.
- Bearer headers are attached per `HttpRequestMessage` – never via `HttpClient.DefaultRequestHeaders`, which would be a race condition across concurrent requests on a shared `HttpClient`.
- Requests that come back `401 Unauthorized` automatically retry once with a freshly minted token before returning failure.

### Configuration

Both applications read the JWT configuration from:

```json
"Jwt": {
  "Key": "<32+ byte random secret>",
  "Issuer": "<your issuer>",
  "Audience": "<optional; defaults to Issuer>"
}
```

And the API key from:

```json
"ApiKey": "<secret shared with external clients>"
```

At startup, `AddInfernoJwtBearer` installs a validation step that:

- Throws if `Jwt:Key` or `Jwt:Issuer` is missing.
- Logs a **warning** (in Development) or **aborts startup** (in Production) if `Jwt:Key` is still the example value shipped in `appsettings.json` or is shorter than 32 bytes.

**Secrets should never be committed to source control.** For local development use user secrets (`dotnet user-secrets set "Jwt:Key" "..."`). In Production use environment variables, Azure Key Vault, AWS Secrets Manager, or equivalent:

```bash
export Jwt__Key="$(openssl rand -base64 48)"
export ApiKey="$(openssl rand -hex 32)"
```

### Identity Policy Defaults

`AddInfernoIdentity` configures:

- **Password policy**: 8 characters minimum, requires upper, lower and digit.
- **Lockout**: 15 minute lockout after 5 failed attempts, applies to new users as well.
- **Account**: confirmed account + unique email required.

### Client Example

```csharp
// Login
var login = await http.PostAsJsonAsync("/auth/login", new {
    apiKey = "...",
    username = "user@example.com",
    password = "..."
});
var tokenResponse = await login.Content.ReadFromJsonAsync<TokenResponse>();
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

// Call API...

// Refresh (before or after expiry)
var refresh = await http.PostAsJsonAsync("/auth/refresh", new {
    apiKey = "...",
    token = tokenResponse.Token
});
```

### Recommended Production Hardening

Items that are intentionally out of scope for the default template but you should consider before shipping:

1. **Rotate the default API key and JWT signing key** – anything that has ever appeared in this repository must be considered compromised.
2. **Persist refresh tokens server-side** with rotation + reuse detection if you need revocation stronger than the security-stamp check in `/auth/refresh`.
3. **Enforce HTTPS** end-to-end and set `HSTS` in production (the CMS already does; make sure any reverse proxy does too).
4. **CORS** – the CMS currently uses `AllowAll`. Tighten this to the real origins that should reach the API.
5. **Audit logging** – send the `Successful API login` / `Failed API login` / `Refreshed token` events from `AuthController` to durable log storage and wire up alerting.
