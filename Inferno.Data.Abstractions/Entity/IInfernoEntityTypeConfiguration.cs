using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inferno.Data.Entity;

/// <summary>An entity map the data registrar can turn on or off.</summary>
public interface IInfernoEntityTypeConfiguration
{
    /// <summary><see langword="true"/> when the registrar should register this map.</summary>
    bool IsEnabled { get; }
}

/// <summary>Base map for an entity stored in the inferno schema. <see cref="Configure"/> is the mapping.</summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class InfernoEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>, IInfernoEntityTypeConfiguration
    where TEntity : class
{
    /// <summary>The schema entity tables are created in.</summary>
    public const string InfernoSchema = "inferno";

    /// <summary><see langword="true"/>, so the registrar includes the map unless a subclass turns it off.</summary>
    public virtual bool IsEnabled => true;

    /// <summary>Maps <typeparamref name="TEntity"/>.</summary>
    /// <param name="builder">The builder for this entity.</param>
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);
}
