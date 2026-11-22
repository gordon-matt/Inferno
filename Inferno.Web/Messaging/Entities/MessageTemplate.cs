using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Messaging.Entities;

/// <summary>A named message template for one tenant. The body lives on <see cref="MessageTemplateVersion"/>.</summary>
public class MessageTemplate : TenantEntity<int>
{
    /// <summary>The name used to find this template.</summary>
    public string Name { get; set; }

    /// <summary>
    /// JSON array of Liquid tokens the editor can insert, such as <c>{{ FirstName }}</c>.
    /// </summary>
    public string Tokens { get; set; }

    /// <summary>An optional owner. <see langword="null"/> when the template is not owned by one record.</summary>
    public Guid? OwnerId { get; set; }

    /// <summary>Whether <see cref="Inferno.Web.Messaging.Services.IMessageService"/> will queue mail from this template. A disabled template queues nothing.</summary>
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