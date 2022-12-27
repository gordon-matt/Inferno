using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Data.Entity
{
    public interface IInfernoEntityTypeConfiguration
    {
        bool IsEnabled { get; }
    }

    public abstract class InfernoEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>, IInfernoEntityTypeConfiguration
        where TEntity : class
    {
        public const string InfernoSchema = "inferno";

        public virtual bool IsEnabled => true;

        public abstract void Configure(EntityTypeBuilder<TEntity> builder);
    }
}