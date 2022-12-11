using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfernoCMS.Data.Entities
{
    public class UserProfileEntry : TenantEntity<int>
    {
        public string UserId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }

    public class UserProfileEntryMap : IEntityTypeConfiguration<UserProfileEntry>, IInfernoEntityTypeConfiguration
    {
        public void Configure(EntityTypeBuilder<UserProfileEntry> builder)
        {
            builder.ToTable(Constants.Tables.UserProfiles);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).IsRequired().HasMaxLength(128).IsUnicode(false);
            builder.Property(x => x.Key).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.Value).IsRequired().IsUnicode(true);
        }

        #region IEntityTypeConfiguration Members

        public bool IsEnabled
        {
            get { return true; }
        }

        #endregion IEntityTypeConfiguration Members
    }
}