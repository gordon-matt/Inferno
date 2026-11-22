using System.ComponentModel.DataAnnotations;

namespace InfernoCMS.Models;

/// <summary>The database, administrator, and sample-data choices posted by the installation form.</summary>
public class InstallationModel
{
    /// <summary>The email address of the first administrator.</summary>
    [Required]
    [EmailAddress]
    [Display(Name = "Admin Email")]
    public string AdminEmail { get; set; }

    /// <summary>The first administrator's password.</summary>
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Admin Password")]
    public string AdminPassword { get; set; }

    /// <summary>The password typed a second time. It must match <see cref="AdminPassword"/>.</summary>
    [DataType(DataType.Password)]
    [Compare("AdminPassword", ErrorMessage = "The password and confirmation password do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }

    /// <summary>Whether the user typed a full connection string instead of the server fields.</summary>
    [Display(Name = "Enter Connection String")]
    public bool EnterConnectionString { get; set; }

    /// <summary>The full connection string, used when <see cref="EnterConnectionString"/> is <see langword="true"/>.</summary>
    [Display(Name = "Connection String")]
    public string ConnectionString { get; set; }

    /// <summary>The SQL Server address. Defaults to the local server.</summary>
    [Display(Name = "Database Server")]
    public string DatabaseServer { get; set; } = ".";

    /// <summary>The database name. Defaults to Inferno.</summary>
    [Display(Name = "Database Name")]
    public string DatabaseName { get; set; } = "Inferno";

    /// <summary>Whether the connection uses integrated security. Defaults to <see langword="true"/>.</summary>
    [Display(Name = "Use Windows Authentication")]
    public bool UseWindowsAuthentication { get; set; } = true;

    /// <summary>The SQL login, used when <see cref="UseWindowsAuthentication"/> is <see langword="false"/>.</summary>
    [Display(Name = "Database Username")]
    public string DatabaseUsername { get; set; }

    /// <summary>The SQL login password, used when <see cref="UseWindowsAuthentication"/> is <see langword="false"/>.</summary>
    [Display(Name = "Database Password")]
    public string DatabasePassword { get; set; }

    /// <summary>Whether installation should create sample data.</summary>
    [Display(Name = "Create Sample Data")]
    public bool CreateSampleData { get; set; }

    /// <summary>The site's default language. Defaults to en-US.</summary>
    public string DefaultLanguage { get; set; } = "en-US";

    /// <summary>
    /// Builds the SQL Server connection string from the posted fields.
    /// </summary>
    /// <returns>
    /// The trimmed <see cref="ConnectionString"/> when <see cref="EnterConnectionString"/> is <see langword="true"/>.
    /// Otherwise a connection string for <see cref="DatabaseServer"/> and <see cref="DatabaseName"/>, using integrated security or the SQL login.
    /// </returns>
    public string BuildConnectionString()
    {
        if (EnterConnectionString)
        {
            return ConnectionString?.Trim();
        }

        if (UseWindowsAuthentication)
        {
            return $"Server={DatabaseServer};Initial Catalog={DatabaseName};Integrated Security=True;Persist Security Info=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        }

        return $"Server={DatabaseServer};Initial Catalog={DatabaseName};User={DatabaseUsername};Password={DatabasePassword};Persist Security Info=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
    }
}
