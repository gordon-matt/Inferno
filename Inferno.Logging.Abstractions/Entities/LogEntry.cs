using Inferno.Data.Entity;
using Inferno.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Logging.Entities;

public class LogEntry : TenantEntity<int>
{
    public DateTime EventDateTime { get; set; }

    public string EventLevel { get; set; }

    public string UserName { get; set; }

    public string MachineName { get; set; }

    public string EventMessage { get; set; }

    public string ErrorSource { get; set; }

    public string ErrorClass { get; set; }

    public string ErrorMethod { get; set; }

    public string ErrorMessage { get; set; }

    public string InnerErrorMessage { get; set; }
}

public class LogEntryMap : InfernoEntityTypeConfiguration<LogEntry>
{
    public override void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("Log", InfernoSchema);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventDateTime).IsRequired();
        builder.Property(m => m.EventLevel).IsRequired().HasMaxLength(16).IsUnicode(false);
        builder.Property(m => m.UserName).HasMaxLength(128).IsUnicode(true);
        builder.Property(m => m.MachineName).HasMaxLength(255).IsUnicode(false);
        builder.Property(m => m.EventMessage).IsUnicode(true);
        builder.Property(m => m.ErrorSource).HasMaxLength(255).IsUnicode(true);
        builder.Property(m => m.ErrorClass).HasMaxLength(512).IsUnicode(false);
        builder.Property(m => m.ErrorMethod).HasMaxLength(255).IsUnicode(false);
        builder.Property(m => m.ErrorMessage).IsUnicode(true);
        builder.Property(m => m.InnerErrorMessage).IsUnicode(true);
    }
}