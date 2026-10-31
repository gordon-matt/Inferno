using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Messaging.Entities;

public class MessageTemplate : TenantEntity<int>
{
    public string Name { get; set; }

    public string Editor { get; set; }

    public Guid? OwnerId { get; set; }

    public bool Enabled { get; set; }
}

public class MessageTemplateMap : InfernoEntityTypeConfiguration<MessageTemplate>
{
    public override void Configure(EntityTypeBuilder<MessageTemplate> builder)
    {
        builder.ToTable("MessageTemplates", InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Editor).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Enabled).IsRequired();
    }
}
