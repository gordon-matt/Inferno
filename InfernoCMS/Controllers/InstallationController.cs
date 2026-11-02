using Inferno.Web;
using Inferno.Web.Infrastructure;
using InfernoCMS.Data;
using InfernoCMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfernoCMS.Controllers;

public class InstallationController : Controller
{
    private readonly IWebHelper webHelper;

    public InstallationController(IWebHelper webHelper)
    {
        this.webHelper = webHelper;
    }

    [HttpGet("/installation")]
    public IActionResult Index()
    {
        if (IsInstalled())
        {
            return Redirect("/");
        }

        return View(new InstallationModel());
    }

    [HttpPost("/installation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(InstallationModel model)
    {
        if (IsInstalled())
        {
            return Redirect("/");
        }

        if (string.IsNullOrWhiteSpace(model.ConnectionString))
        {
            ModelState.AddModelError(nameof(model.ConnectionString), "A connection string is required.");
        }

        if (string.IsNullOrWhiteSpace(model.AdminEmail) || !model.AdminEmail.Contains('@'))
        {
            ModelState.AddModelError(nameof(model.AdminEmail), "Enter the administrator email address.");
        }

        if (string.IsNullOrWhiteSpace(model.AdminPassword) || model.AdminPassword.Length < 6)
        {
            ModelState.AddModelError(nameof(model.AdminPassword), "The password must be at least 6 characters.");
        }

        if (!string.Equals(model.AdminPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "The passwords do not match.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(model.ConnectionString)
                .Options;
            await using var context = new ApplicationDbContext(options);
            await context.Database.MigrateAsync();

            var settings = new DataSettings
            {
                ProviderName = "SqlServer",
                ConnectionString = model.ConnectionString,
                AdminEmail = model.AdminEmail,
                AdminPassword = model.AdminPassword,
                DefaultLanguage = string.IsNullOrWhiteSpace(model.DefaultLanguage) ? "en-US" : model.DefaultLanguage,
                Theme = "Default",
                CreateSampleData = model.CreateSampleData
            };
            DataSettingsManager.SaveSettings(settings);
            DataSettingsHelper.ResetCache();

            if (!context.Tenants.Any())
            {
                context.Seed();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);
            return View(model);
        }

        Response.OnCompleted(() =>
        {
            webHelper.RestartAppDomain();
            return Task.CompletedTask;
        });

        return View("Complete");
    }

    private bool IsInstalled()
    {
        var settings = DataSettingsManager.LoadSettings();
        return !string.IsNullOrWhiteSpace(settings.ConnectionString);
    }
}
