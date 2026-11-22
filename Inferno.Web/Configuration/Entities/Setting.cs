using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Configuration.Entities;

/// <summary>One stored settings document. <see cref="Type"/> is the lookup key.</summary>
public class Setting : TenantEntity<Guid>
{
    /// <summary>The label from <see cref="Inferno.Web.Configuration.ISettings.Name"/>.</summary>
    public string Name { get; set; }

    /// <summary>The full name of the settings type.</summary>
    public string Type { get; set; }

    /// <summary>The settings object serialized as JSON.</summary>
    public string Value { get; set; }

    /// <summary>Returns <see cref="Name"/>.</summary>
    /// <returns>The display name.</returns>
    public override string ToString() => Name;
}

public class SettingMap : InfernoEntityTypeConfiguration<Setting>
{
    public override void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings", InfernoSchema);
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(s => s.Type).IsRequired().HasMaxLength(255).IsUnicode(false);
        builder.Property(s => s.Value).IsUnicode(true);
    }
}