﻿using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Tasks.Entities
{
    public class ScheduledTaskMap : InfernoEntityTypeConfiguration<ScheduledTask>
    {
        public override void Configure(EntityTypeBuilder<ScheduledTask> builder)
        {
            builder.ToTable("ScheduledTasks", InfernoSchema);
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(s => s.Type).IsRequired().HasMaxLength(255).IsUnicode(false);
            builder.Property(s => s.Seconds).IsRequired();
            builder.Property(s => s.Enabled).IsRequired();
            builder.Property(s => s.StopOnError).IsRequired();
        }
    }
}