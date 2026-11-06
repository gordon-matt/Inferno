using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Messaging.Entities;

public class MessageTemplate : TenantEntity<int>
{
    public string Name { get; set; }

    /// <summary>
    /// JSON array of Liquid tokens the editor can insert, such as <c>{{ FirstName }}</c>.
    /// </summary>
    public string Tokens { get; set; }

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
        builder.Property(x => x.Tokens).IsUnicode(true);
        builder.Property(x => x.Enabled).IsRequired();
    }
}