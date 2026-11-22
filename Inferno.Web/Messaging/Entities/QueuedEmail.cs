using System.Net.Mail;
using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Messaging.Entities;

/// <summary>A message waiting to be sent. The stored JSON is what <see cref="Inferno.Web.Messaging.MailMessageWrapper"/> rebuilds into mail.</summary>
public class QueuedEmail : TenantEntity<Guid>, IMailMessage
{
    /// <summary>Lower values are sent first. Messages queued through <see cref="Inferno.Web.Messaging.Services.IMessageService"/> use 5.</summary>
    public int Priority { get; set; }

    /// <summary>The From address copied onto the row.</summary>
    public string FromAddress { get; set; }

    /// <summary>The From display name copied onto the row.</summary>
    public string FromName { get; set; }

    /// <summary>The first recipient's address.</summary>
    public string ToAddress { get; set; }

    /// <summary>The first recipient's display name.</summary>
    public string ToName { get; set; }

    /// <summary>The subject copied onto the row.</summary>
    public string Subject { get; set; }

    /// <summary>The JSON <see cref="Inferno.Web.Messaging.MailMessageWrapper"/> used to rebuild the mail message.</summary>
    public string MailMessage { get; set; }

    /// <summary>When the message was queued, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>How many send attempts have failed. Incremented by <see cref="Inferno.Web.Messaging.IQueuedMessageProvider.OnSendError"/>.</summary>
    public int SentTries { get; set; }

    /// <summary>When the message was sent, in UTC. <see langword="null"/> while it is still queued.</summary>
    public DateTime? SentOnUtc { get; set; }

    /// <inheritdoc />
    public MailMessage GetMailMessage() => MailMessageWrapper.FromJson(MailMessage).ToMailMessage();
}

public class QueuedEmailMap : InfernoEntityTypeConfiguration<QueuedEmail>
{
    public override void Configure(EntityTypeBuilder<QueuedEmail> builder)
    {
        builder.ToTable("QueuedEmails", InfernoSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.FromAddress).HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.FromName).HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.ToAddress).IsRequired().HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.ToName).HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Subject).HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.MailMessage).IsRequired().IsUnicode(true);
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.SentTries).IsRequired();
    }
}
