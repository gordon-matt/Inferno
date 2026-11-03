# Security Documentation

## Authentication & Authorization

### Overview

**InfernoCMS** is a single ASP.NET Core application. The administration area is an Interactive Server Blazor app that authenticates with ASP.NET Core Identity and cookie authentication. Admin pages call their services directly inside the Blazor circuit, so there is no separate API, bearer token or API key to secure.

The only admin HTTP endpoints are a few thin MVC controllers for downloads that a Blazor circuit cannot stream (CSV/XLSX exports). They use the same cookie and the same authorization policies as the pages that link to them:

| Endpoint | Policy |
| --- | --- |
| `admin/tenants/export/{format}` | `FullAccess` |
| `admin/newsletters/subscribers/export/csv` | Newsletter read |

### Authorization

- Every admin page declares a read policy with `[Authorize(Policy = ...)]`.
- Write actions check the matching write policy again on the server before changing data, so a hidden button is never the only protection.
- Admin services scope every query and change to the current tenant, and reject records that belong to another tenant.
- Members of the built-in `Administrators` role satisfy every policy. Other users are granted policies through permission claims.

### Identity Policy Defaults

`AddInfernoIdentity` configures:

- **Password policy**: 8 characters minimum, requires upper, lower and digit.
- **Lockout**: 15 minute lockout after 5 failed attempts, applies to new users as well.
- **Account**: confirmed account + unique email required.

### Recommended Production Hardening

Items that are intentionally out of scope for the default template but you should consider before shipping:

1. **Change the seeded administrator password** immediately after installation.
2. **Enforce HTTPS** end-to-end and set `HSTS` in production (the CMS already does; make sure any reverse proxy does too).
3. **CORS**: the CMS currently uses `AllowAll`. Tighten this to the real origins that need it, or remove it if nothing cross-origin calls the site.
4. **Secrets** such as connection strings and SMTP credentials should never be committed to source control. Use user secrets for development and environment variables or a secret store in production.
