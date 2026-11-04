[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

<img src="https://github.com/gordon-matt/Inferno/raw/master/InfernoCMS/wwwroot/img/Logo_Dark.png" alt="Logo" width="350" />

Inferno is a content management system and application framework for ASP.NET Core, built on .NET 10
and Blazor. Use it as a ready-made CMS, or as the base for a site that needs pages, menus, users and
an admin area without building them from scratch.

## Features

- **Pages and content.** A page tree with drag-and-drop page design, version history with restore,
  content blocks and zones, menus, and a media library with a file manager and rich text editor.
- **Blog.** Posts, categories and tags, edited in the same admin area.
- **Multi-tenant.** One installation serves several sites, each with its own pages, settings,
  users and roles, chosen by host name.
- **Users, roles and permissions.** ASP.NET Core Identity with a full management UI. Permissions are
  claims, granted to roles or to individual users, and every module adds its own.
- **Localization.** Manage languages and translate the site's text from the admin area.
- **Marketing and messaging.** Newsletter subscribers, email templates, a queued email sender, and
  an XML sitemap.
- **Themes and settings.** Switch the site's theme, and edit each module's settings in one place.
  The admin area has light and dark modes.
- **Maintenance.** A log viewer, and [Hangfire](https://www.hangfire.io/) for background jobs. The host
  administrator sets how often they run under Settings. The job dashboard covers every tenant.

## Tech

<p align="left">
    <a href="https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/Inferno/raw/master/_SolutionItems/Logos/Blazor.jpg" alt="Blazor" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/PageKrafter" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/PageKrafter/raw/master/_Misc/Logo.png" alt="PageKrafter" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/BlogKrafter" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/BlogKrafter/raw/master/_Misc/Logo.png" alt="BlogKrafter" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/IdentityNexus" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/IdentityNexus/raw/master/_Misc/Logo.png" alt="IdentityNexus" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/Pluginz" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/Pluginz/raw/master/_Misc/Logo.png" alt="Pluginz" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/LogVue" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/LogVue/raw/master/_Misc/Logo.png" alt="LogVue" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/WebFilez" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/WebFilez/blob/master/_Misc/Logo-Light.png" alt="WebFilez" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/Dependo" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/Dependo/raw/master/_Misc/Logo.png" alt="Dependo" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/Extenso" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/Extenso/raw/master/_Misc/ExtensoLogo.png" alt="Extenso" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/Pickstack" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/Pickstack/raw/master/_Misc/Logo.png" alt="Pickstack" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/Flatmate" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/Flatmate/raw/master/_Misc/Logo.png" alt="Flatmate" height="60"/>
    </a>&nbsp;
    <a href="https://github.com/gordon-matt/Heroicsoft.NetForge" target="_blank" rel="noreferrer">
        <img src="https://github.com/gordon-matt/Heroicsoft.NetForge/raw/master/_Misc/Logo-Light.png" alt=".NetForge" height="60"/>
    </a>
</p>

## Modular by design

Almost everything in Inferno is a separate module, and the core knows very little about any of them.

- **Registrars.** Each project wires itself up with a [Dependo](https://github.com/gordon-matt/Dependo)
  dependency registrar, so adding a module to the host is mostly a project reference.
- **Plugins.** Plugins are NuGet packages tagged `inferno-cms`. They are browsed, installed and removed
  from the admin area through [Pluginz](https://github.com/gordon-matt/Pluginz), and loaded at startup.
- **Permissions.** A module declares its permissions by implementing `IPermissionProvider`. They appear
  in the users and roles editors and can be used as authorization policies by name, with no
  extra registration.
- **Settings.** A module's settings are a class plus an editor component, discovered at startup.
- **Reusable libraries.** The larger features are standalone libraries that work in any ASP.NET Core
  site, not only Inferno:

| Library | What it does |
| --- | --- |
| [PageKrafter](https://github.com/gordon-matt/PageKrafter) | Drag-and-drop page designer |
| [BlogKrafter](https://github.com/gordon-matt/BlogKrafter) | Blog engine and admin |
| [IdentityNexus](https://github.com/gordon-matt/IdentityNexus) | Users and roles management UI for ASP.NET Core Identity |
| [Localize.NET](https://github.com/gordon-matt/Localize.NET) | Database-backed localization with a translation UI |
| [LogVue](https://github.com/gordon-matt/LogVue) | Log viewer |
| [Pickstack](https://github.com/gordon-matt/Pickstack) | File manager and rich text editor integration |
| [Pluginz](https://github.com/gordon-matt/Pluginz) | Plugin discovery, installation and loading |
| [NetForge](https://github.com/gordon-matt/Heroicsoft.NetForge) | Blazor UI components and the admin shell |
| [Extenso](https://github.com/gordon-matt/Extenso) | Data access, repositories and helpers |

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express or full)

### Run it

1. Clone this repository. The solution references the libraries above by relative path, so clone
   them into a `Libraries` folder two levels up (for example `D:\Source\GitHub\Libraries` next to
   `D:\Source\GitHub\CMS\Inferno`).
2. Run the site:

   ```bash
   dotnet run --project InfernoCMS --launch-profile http
   ```

3. Open `http://localhost:5263`. The first run takes you to `/installation`. Choose the database
   server and name (or paste a connection string), create the administrator account, and optionally
   add sample data. The database and default tenant are created for you, and the connection settings
   are saved to `App_Data/InfernoSettings.xml`.
4. Sign in and go to `/admin`.

### Next steps

- Add users and roles under **Membership**, and give roles permissions on their **Permissions** tab.
  Members of the Administrators role already hold everything.
- Add another tenant under **Configuration > Tenants**, with the host names it should answer to.
- Install plugins under **Configuration > Plugins**.

## Support

If you find this project useful, consider supporting its development.

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_Logo.png"
         alt="Buy Me a Coffee"
         height="40">
  </a>&nbsp;&nbsp;&nbsp;
  <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/PayPal_Logo.png"
         alt="Donate with PayPal"
         height="40">
  </a>
</p>

<p align="center">
  Prefer using your phone? Scan the QR code:
</p>

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_QR.png"
         alt="Buy Me a Coffee QR Code"
         width="180">
  </a>
</p>

<img src="https://komarev.com/ghpvc/?username=gordon-matt&label=GitHub%20Hits%20Since%202025-06-01%3A%20&color=ff0000&style=flat" alt="gordon-matt" />
