using Flatmate;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using InfernoCMS.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfernoCMS.Controllers;

/// <summary>Lets the host administrator open one tenant's site from this host.</summary>
[Authorize]
[Route("host/tenant")]
public class HostTenantController(ITenantService tenantService) : Controller
{
    /// <summary>Lists every tenant so the host administrator can choose one. Anyone else is not found.</summary>
    /// <returns>The chooser, or not found.</returns>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!await HostTenantGate.IsHostAdministratorAsync(HttpContext))
        {
            return NotFound();
        }

        var tenants = tenantService.Find(new Extenso.Data.Entity.SearchOptions<Tenant>
        {
            Query = tenant => true
        }).OrderBy(tenant => tenant.Name).ToList();

        return View(tenants);
    }

    /// <summary>Stores <paramref name="id"/> as the tenant this browser should show, then returns to <paramref name="returnUrl"/>.</summary>
    /// <param name="id">The tenant. An unknown id clears the selection.</param>
    /// <param name="returnUrl">A local address to open afterwards. Anything else opens the site root.</param>
    /// <returns>A redirect, or not found when the caller is not the host administrator.</returns>
    [HttpGet("use/{id:int}")]
    public async Task<IActionResult> Use(int id, string returnUrl)
    {
        if (!await HostTenantGate.IsHostAdministratorAsync(HttpContext))
        {
            return NotFound();
        }

        var tenant = await tenantService.FindOneAsync(id);
        if (tenant is null)
        {
            HostTenantCookie.Clear(HttpContext);
        }
        else
        {
            Response.Cookies.Append(HostTenantCookie.Name, tenant.Id.ToString(), HostTenantCookie.Options(Request));
        }

        return Redirect(Local(returnUrl));
    }

    /// <summary>Forgets the selected tenant, so the host name chooses the site again.</summary>
    /// <param name="returnUrl">A local address to open afterwards. Anything else opens the site root.</param>
    /// <returns>A redirect, or not found when the caller is not the host administrator.</returns>
    [HttpGet("clear")]
    public async Task<IActionResult> Clear(string returnUrl)
    {
        if (!await HostTenantGate.IsHostAdministratorAsync(HttpContext))
        {
            return NotFound();
        }

        HostTenantCookie.Clear(HttpContext);
        return Redirect(Local(returnUrl));
    }

    private string Local(string returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
}
