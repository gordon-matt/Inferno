using System.Linq.Expressions;
using Dependo;
using Extenso.Collections.Generic;
using Extenso.Data.Entity;
using Inferno.Caching;
using Microsoft.Extensions.Logging;

namespace Inferno.Data.Services;

/// <inheritdoc cref="IGenericDataService{TEntity}" />
public class GenericDataService<TEntity> : IGenericDataService<TEntity> where TEntity : class
{
    #region Private Members

    private static string cacheKey;
    private static string cacheKeyFiltered;
    private readonly IRepository<TEntity> repository;

    #endregion Private Members

    #region Properties

    /// <summary>Cache key for the unfiltered page. One key per closed entity type, filled on first use as Repository_ and the entity name.</summary>
    protected virtual string CacheKey
    {
        get
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = string.Format("Repository_{0}", typeof(TEntity).Name);
            }
            return cacheKey;
        }
    }

    /// <summary>Format for a primary-key cache entry. The placeholder is the key values joined with a hyphen.</summary>
    protected virtual string CacheKeyFiltered
    {
        get
        {
            if (string.IsNullOrEmpty(cacheKeyFiltered))
            {
                cacheKeyFiltered = string.Format("Repository_{0}_{{0}}", typeof(TEntity).Name);
            }
            return cacheKeyFiltered;
        }
    }

    /// <summary>The cache the finds and the writes use.</summary>
    public ICacheManager CacheManager { get; private set; }

    /// <summary>Logger created from the dependency resolver for this closed entity type.</summary>
    public ILogger Logger { get; private set; }

    #endregion Properties

    #region Constructor

    /// <summary>Stores the cache and the repository, and creates <see cref="Logger"/> from the dependency resolver.</summary>
    /// <param name="cacheManager">The cache used by the unfiltered find and the primary-key find.</param>
    /// <param name="repository">The store the queries and writes go to.</param>
    public GenericDataService(
        ICacheManager cacheManager,
        IRepository<TEntity> repository)
    {
        CacheManager = cacheManager;
        this.repository = repository;

        var loggerFactory = DependoResolver.Instance.Resolve<ILoggerFactory>();
        Logger = loggerFactory.CreateLogger<GenericDataService<TEntity>>();
    }

    #endregion Constructor

    #region IGenericDataService<TEntity> Members

    #region Find

    /// <inheritdoc />
    public virtual IPagedCollection<TEntity> Find(SearchOptions<TEntity> options)
    {
        if (options?.Query != null)
        {
            return repository.Find(options);
        }

        return CacheManager.Get(CacheKey, () => repository.Find(options ?? new SearchOptions<TEntity>()));
    }

    /// <inheritdoc />
    public virtual IPagedCollection<TResult> Find<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        repository.Find(options, projection);

    /// <inheritdoc />
    public virtual Task<IPagedCollection<TEntity>> FindAsync(SearchOptions<TEntity> options)
    {
        if (options?.Query != null)
        {
            return repository.FindAsync(options);
        }

        return Task.FromResult(CacheManager.Get(CacheKey, () => repository.Find(options ?? new SearchOptions<TEntity>())));
    }

    /// <inheritdoc />
    public virtual Task<IPagedCollection<TResult>> FindAsync<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        repository.FindAsync(options, projection);

    /// <inheritdoc />
    public virtual TEntity FindOne(params object[] keyValues)
    {
        string key = string.Format(CacheKeyFiltered, string.Join("-", keyValues.Select(x => x ?? "null")));
        return CacheManager.Get(key, () => repository.FindOne(keyValues));
    }

    /// <inheritdoc />
    public virtual TEntity FindOne(SearchOptions<TEntity> options) =>
        repository.FindOne(options);

    /// <inheritdoc />
    public virtual TResult FindOne<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        repository.FindOne(options, projection);

    /// <inheritdoc />
    public virtual Task<TEntity> FindOneAsync(params object[] keyValues)
    {
        string key = string.Format(CacheKeyFiltered, string.Join("-", keyValues.Select(x => x ?? "null")));
        return Task.FromResult(CacheManager.Get(key, () => repository.FindOne(keyValues)));
    }

    /// <inheritdoc />
    public virtual Task<TEntity> FindOneAsync(SearchOptions<TEntity> options) =>
        repository.FindOneAsync(options);

    /// <inheritdoc />
    public virtual Task<TResult> FindOneAsync<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        repository.FindOneAsync(options, projection);

    #endregion Find

    #region Open/Use Connection

    /// <inheritdoc />
    public virtual IRepositoryConnection<TEntity> OpenConnection() => repository.OpenConnection();

    /// <inheritdoc />
    public virtual IRepositoryConnection<TEntity> UseConnection<TOther>(IRepositoryConnection<TOther> connection)
        where TOther : class => repository.UseConnection(connection);

    #endregion Open/Use Connection

    #region Count

    /// <inheritdoc />
    public virtual int Count() => repository.Count();

    /// <inheritdoc />
    public virtual int Count(Expression<Func<TEntity, bool>> countExpression) => repository.Count(countExpression);

    /// <inheritdoc />
    public virtual async Task<int> CountAsync() => await repository.CountAsync();

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> countExpression) => await repository.CountAsync(countExpression);

    #endregion Count

    #region Delete

    /// <inheritdoc />
    public virtual int DeleteAll()
    {
        int rowsAffected = repository.DeleteAll();
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual int Delete(TEntity entity)
    {
        int rowsAffected = repository.Delete(entity);
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual int Delete(IEnumerable<TEntity> entities)
    {
        int rowsAffected = repository.Delete(entities);
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual int Delete(Expression<Func<TEntity, bool>> filterExpression)
    {
        int rowsAffected = repository.Delete(filterExpression);
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual int Delete(IQueryable<TEntity> query)
    {
        int rowsAffected = repository.Delete(query);
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAllAsync()
    {
        int rowsAffected = await repository.DeleteAllAsync();
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAsync(TEntity entity)
    {
        int rowsAffected = await repository.DeleteAsync(entity);
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAsync(IEnumerable<TEntity> entities)
    {
        int rowsAffected = await repository.DeleteAsync(entities);
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> filterExpression)
    {
        int rowsAffected = await repository.DeleteAsync(filterExpression);
        ClearCache();
        return rowsAffected;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAsync(IQueryable<TEntity> query)
    {
        int rowsAffected = await repository.DeleteAsync(query);
        ClearCache();
        return rowsAffected;
    }

    #endregion Delete

    #region Insert

    /// <inheritdoc />
    public virtual TEntity Insert(TEntity entity)
    {
        entity = repository.Insert(entity);
        ClearCache();
        return entity;
    }

    /// <inheritdoc />
    public virtual IEnumerable<TEntity> Insert(IEnumerable<TEntity> entities)
    {
        entities = repository.Insert(entities);
        ClearCache();
        return entities;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> InsertAsync(TEntity entity)
    {
        entity = await repository.InsertAsync(entity);
        ClearCache();
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> InsertAsync(IEnumerable<TEntity> entities)
    {
        entities = await repository.InsertAsync(entities);
        ClearCache();
        return entities;
    }

    #endregion Insert

    #region Update

    /// <inheritdoc />
    public virtual TEntity Update(TEntity entity)
    {
        entity = repository.Update(entity);
        ClearCache();
        return entity;
    }

    /// <inheritdoc />
    public virtual IEnumerable<TEntity> Update(IEnumerable<TEntity> entities)
    {
        entities = repository.Update(entities);
        ClearCache();
        return entities;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        entity = await repository.UpdateAsync(entity);
        ClearCache();
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> UpdateAsync(IEnumerable<TEntity> entities)
    {
        entities = await repository.UpdateAsync(entities);
        ClearCache();
        return entities;
    }

    #endregion Update

    #endregion IGenericDataService<TEntity> Members

    /// <summary>Drops the unfiltered page and every primary-key entry for this entity.</summary>
    protected virtual void ClearCache()
    {
        CacheManager.Remove(CacheKey);
        CacheManager.RemoveByPattern(string.Format(CacheKeyFiltered, ".*"));
    }
}