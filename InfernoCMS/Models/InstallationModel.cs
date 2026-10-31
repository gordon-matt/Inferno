namespace InfernoCMS.Models;

public class InstallationModel
{
    public string ConnectionString { get; set; }

    public string AdminEmail { get; set; }

    public string AdminPassword { get; set; }

    public string ConfirmPassword { get; set; }

    public string DefaultLanguage { get; set; } = "en-US";

    public bool CreateSampleData { get; set; }
}
