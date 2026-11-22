using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfernoCMS.Data.Entities;

/// <summary>One profile value stored for a user in a tenant.</summary>
public class UserProfileEntry : TenantEntity<int>
{
    /// <summary>The Identity user id this value belongs to.</summary>
    public string UserId { get; set; }

    /// <summary>The profile field name.</summary>
    public string Key { get; set; }

    /// <summary>The stored field value.</summary>
    public string Value { get; set; }
}

public class UserProfileEntryMap : IEntityTypeConfiguration<UserProfileEntry>, IInfernoEntityTypeConfiguration
{
    public void Configure(EntityTypeBuilder<UserProfileEntry> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(128).IsUnicode(false);
        builder.Property(x => x.Key).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Value).IsRequired().IsUnicode(true);
    }

    #region IEntityTypeConfiguration Members

    public bool IsEnabled => true;

    #endregion IEntityTypeConfiguration Members
}