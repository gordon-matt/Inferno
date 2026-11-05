using Ardalis.Result;
using Flatmate;
using Inferno.Helpers;
using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Inferno.Web.Models;
using Inferno.Web.Tenants;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Areas.Admin.Tenants.Services;

public interface ITenantAdminService
{
    Task<Result<ListPage<Tenant>>> SearchAsync(ListQuery query);

    /// <summary>Every tenant matching <paramref name="search"/>, for an export.</summary>
    Task<Result<IReadOnlyList<Tenant>>> ListAsync(string search);

    Task<Result<Tenant>> GetAsync(int id);

    /// <summary>
    /// Inserts when <see cref="Tenant.Id"/> is 0, otherwise updates. A new tenant also gets its
    /// administrator role and media folder, which nothing else creates later.
    /// </summary>
    Task<Result<Tenant>> SaveAsync(Tenant tenant);

    /// <summary>Removes the tenant and everything stored for it.</summary>
    Task<Result> DeleteAsync(int id);
}

public class TenantAdminService(
    ITenantService tenantService,
    IMembershipService membershipService,
    ITenantDataCleaner tenantDataCleaner,
    ITenantCache<Tenant> tenantCache,
    ILogger<TenantAdminService> logger) : ITenantAdminService
{
    /// <summary>Forgets what the resolver remembered for these hosts, so a new, changed or removed tenant takes effect now instead of after the cache expires.</summary>
    private void EvictHosts(params string[] hostLists)
    {
        foreach (string host in hostLists
            .Where(list => !string.IsNullOrWhiteSpace(list))
            .SelectMany(list => list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
        {
            tenantCache.Evict(host.ToLowerInvariant());
        }
    }

    public Task<Result<ListPage<Tenant>>> SearchAsync(ListQuery query)
    {
        using var connection = tenantService.OpenConnection();
        var filtered = Filter(connection.Query(), query.Search);

        int total = filtered.Count();
        var items = Sort(filtered, query.SortField, query.SortDescending)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToList();

        return Task.FromResult(Result.Success(new ListPage<Tenant>(items, total)));
    }

    public Task<Result<IReadOnlyList<Tenant>>> ListAsync(string search)
    {
        using var connection = tenantService.OpenConnection();
        IReadOnlyList<Tenant> items = Filter(connection.Query(), search).OrderBy(tenant => tenant.Name).ToList();
        return Task.FromResult(Result.Success(items));
    }

    public async Task<Result<Tenant>> GetAsync(int id)
    {
        var tenant = await tenantService.FindOneAsync(id);
        return tenant is null ? Result<Tenant>.NotFound() : Result.Success(tenant);
    }

    public async Task<Result<Tenant>> SaveAsync(Tenant tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.Name))
        {
            return Result<Tenant>.Invalid(new ValidationError { Identifier = nameof(Tenant.Name), ErrorMessage = "Enter a name." });
        }

        if (string.IsNullOrWhiteSpace(tenant.Url))
        {
            return Result<Tenant>.Invalid(new ValidationError { Identifier = nameof(Tenant.Url), ErrorMessage = "Enter the site URL." });
        }

        try
        {
            if (tenant.Id != 0)
            {
                var existing = await tenantService.FindOneAsync(tenant.Id);
                if (existing is null)
                {
                    return Result<Tenant>.NotFound();
                }

                string previousHosts = existing.Hosts;
                existing.Name = tenant.Name.Trim();
                existing.Url = tenant.Url.Trim();
                existing.Hosts = tenant.Hosts?.Trim();
                existing.SslEnabled = tenant.SslEnabled;
                existing.SecureUrl = tenant.SecureUrl?.Trim();
                await tenantService.UpdateAsync(existing);
                EvictHosts(previousHosts, existing.Hosts);
                return Result.Success(existing);
            }

            var created = await tenantService.InsertAsync(new Tenant
            {
                Name = tenant.Name.Trim(),
                Url = tenant.Url.Trim(),
                Hosts = tenant.Hosts?.Trim(),
                SslEnabled = tenant.SslEnabled,
                SecureUrl = tenant.SecureUrl?.Trim(),
            });

            await membershipService.EnsureAdminRoleForTenantAsync(created.Id);
            EvictHosts(created.Hosts);

            var mediaFolder = new DirectoryInfo(CommonHelper.MapPath($"~/Media/Uploads/Tenant_{created.Id}"));
            if (!mediaFolder.Exists)
            {
                mediaFolder.Create();
            }

            return Result.Success(created);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save tenant {TenantId}", tenant.Id);
            return Result<Tenant>.Error("The tenant could not be saved.");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var tenant = await tenantService.FindOneAsync(id);
        if (tenant is null)
        {
            return Result.NotFound();
        }

        try
        {
            await tenantDataCleaner.CleanAsync(id);
            await tenantService.DeleteAsync(tenant);
            EvictHosts(tenant.Hosts);
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete tenant {TenantId}", id);
            return Result.Error("The tenant could not be deleted.");
        }
    }

    private static IQueryable<Tenant> Filter(IQueryable<Tenant> query, string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string term = search.Trim();
        return query.Where(tenant => tenant.Name.Contains(term) || tenant.Url.Contains(term) || tenant.Hosts.Contains(term));
    }

    private static IQueryable<Tenant> Sort(IQueryable<Tenant> query, string field, bool descending) => field switch
    {
        nameof(Tenant.Url) => descending ? query.OrderByDescending(tenant => tenant.Url) : query.OrderBy(tenant => tenant.Url),
        nameof(Tenant.Id) => descending ? query.OrderByDescending(tenant => tenant.Id) : query.OrderBy(tenant => tenant.Id),
        _ => descending ? query.OrderByDescending(tenant => tenant.Name) : query.OrderBy(tenant => tenant.Name),
    };
}
