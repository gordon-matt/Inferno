using Extenso.Data.Entity;
using Inferno.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfernoCMS.Data.Entities
{
    public class Person : BaseEntity<int>
    {
        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public DateTime DateOfBirth { get; set; }
    }

    public class PersonMap : IEntityTypeConfiguration<Person>, IInfernoEntityTypeConfiguration
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("People");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.FamilyName).IsRequired().HasMaxLength(128).IsUnicode(true);
            builder.Property(m => m.GivenNames).IsRequired().HasMaxLength(128).IsUnicode(true);
            builder.Property(m => m.DateOfBirth).IsRequired();
        }

        public bool IsEnabled => true;
    }
}