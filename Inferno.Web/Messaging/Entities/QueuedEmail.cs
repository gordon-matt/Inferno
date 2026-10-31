using System.Net.Mail;
using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Web.Messaging.Entities;

public class QueuedEmail : TenantEntity<Guid>, IMailMessage
{
    public int Priority { get; set; }

    public string FromAddress { get; set; }

    public string FromName { get; set; }

    public string ToAddress { get; set; }

    public string ToName { get; set; }

    public string Subject { get; set; }

    public string MailMessage { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public int SentTries { get; set; }

    public DateTime? SentOnUtc { get; set; }

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
