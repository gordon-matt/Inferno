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

        if (model.EnterConnectionString)
        {
            if (string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                ModelState.AddModelError(nameof(model.ConnectionString), "A connection string is required.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.DatabaseServer))
            {
                ModelState.AddModelError(nameof(model.DatabaseServer), "The database server is required.");
            }

            if (string.IsNullOrWhiteSpace(model.DatabaseName))
            {
                ModelState.AddModelError(nameof(model.DatabaseName), "The database name is required.");
            }

            if (!model.UseWindowsAuthentication && string.IsNullOrWhiteSpace(model.DatabaseUsername))
            {
                ModelState.AddModelError(nameof(model.DatabaseUsername), "The database username is required.");
            }
        }

        if (string.IsNullOrWhiteSpace(model.AdminEmail) || !model.AdminEmail.Contains('@'))
        {
            ModelState.AddModelError(nameof(model.AdminEmail), "Enter the administrator email address.");
        }

        if (string.IsNullOrWhiteSpace(model.AdminPassword) || !IsStrongPassword(model.AdminPassword))
        {
            ModelState.AddModelError(nameof(model.AdminPassword), "The password must be at least 8 characters and include an uppercase letter, a lowercase letter and a number.");
        }

        if (!string.Equals(model.AdminPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "The passwords do not match.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string connectionString = model.BuildConnectionString();

        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;
            await using var context = new ApplicationDbContext(options);
            await context.Database.MigrateAsync();

            var settings = new DataSettings
            {
                ProviderName = "SqlServer",
                ConnectionString = connectionString,
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

    private static bool IsStrongPassword(string password) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= 8
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit);

    private bool IsInstalled()
    {
        var settings = DataSettingsManager.LoadSettings();
        return !string.IsNullOrWhiteSpace(settings.ConnectionString);
    }
}
