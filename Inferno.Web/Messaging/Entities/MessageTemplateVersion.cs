using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Messaging.Entities;

public class MessageTemplateVersion : BaseEntity<int>
{
    public int MessageTemplateId { get; set; }

    public string CultureCode { get; set; }

    public string Subject { get; set; }

    /// <summary>
    /// HTML body, or a PageKrafter document. Liquid tokens such as {{ FirstName }} are filled when the message is queued.
    /// </summary>
    public string Data { get; set; }

    public DateTime DateCreatedUtc { get; set; }

    public DateTime DateModifiedUtc { get; set; }

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
