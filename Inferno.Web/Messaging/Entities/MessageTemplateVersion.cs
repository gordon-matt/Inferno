using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Messaging.Entities;

/// <summary>The subject and body of a <see cref="MessageTemplate"/> for one culture.</summary>
public class MessageTemplateVersion : BaseEntity<int>
{
    /// <summary>The template this version belongs to.</summary>
    public int MessageTemplateId { get; set; }

    /// <summary>The culture of this version. <see langword="null"/> is the fallback when the requested culture has no version.</summary>
    public string CultureCode { get; set; }

    /// <summary>The subject. Liquid tokens are filled without HTML encoding when the message is queued.</summary>
    public string Subject { get; set; }

    /// <summary>
    /// HTML body, or a PageKrafter document. Liquid tokens such as {{ FirstName }} are filled when the message is queued.
    /// </summary>
    public string Data { get; set; }

    /// <summary>When this version was created, in UTC.</summary>
    public DateTime DateCreatedUtc { get; set; }

    /// <summary>When this version was last changed, in UTC.</summary>
    public DateTime DateModifiedUtc { get; set; }

    /// <summary>The template this version belongs to.</summary>
    public virtual MessageTemplate MessageTemplate { get; set; }
}

public class MessageTemplateVersionMap : InfernoEntityTypeConfiguration<MessageTemplateVersion>
{
    public override void Configure(EntityTypeBuilder<MessageTemplateVersion> builder)
    {
        builder.ToTable("MessageTemplateVersions", InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MessageTemplateId).IsRequired();
        builder.Property(x => x.CultureCode).HasMaxLength(10).IsUnicode(false);
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Data).IsUnicode(true);
        builder.Property(x => x.DateCreatedUtc).IsRequired();
        builder.Property(x => x.DateModifiedUtc).IsRequired();
        builder.HasOne(x => x.MessageTemplate).WithMany().HasForeignKey(x => x.MessageTemplateId);
        builder.HasIndex(x => x.MessageTemplateId);
    }
}
