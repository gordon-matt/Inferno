using System.Linq.Expressions;
using Extenso.Collections.Generic;
using Extenso.Data.Entity;

namespace Inferno.Data.Services;

/// <summary>
/// Reads and writes one entity set.
/// <see cref="GenericDataService{TEntity}"/> caches an unfiltered find and a find by primary key for 60 minutes, and drops that cache after an insert, update, or delete.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IGenericDataService<TEntity> where TEntity : class
{
    #region Open Connection

    /// <summary>Opens a repository connection for a query this service does not cover.</summary>
    /// <returns>An open connection for <typeparamref name="TEntity"/>.</returns>
    IRepositoryConnection<TEntity> OpenConnection();

    /// <summary>Runs against a connection another entity already opened.</summary>
    /// <typeparam name="TOther">The entity type that owns <paramref name="connection"/>.</typeparam>
    /// <param name="connection">The connection to share.</param>
    /// <returns>A connection for <typeparamref name="TEntity"/> on the same store session.</returns>
    IRepositoryConnection<TEntity> UseConnection<TOther>(IRepositoryConnection<TOther> connection)
        where TOther : class;

    #endregion Open Connection

    #region Find

    /// <summary>
    /// Returns a page of entities. A call whose <paramref name="options"/> have a query is read from the store.
    /// <see langword="null"/> options, or options whose query is <see langword="null"/>, return the cached unfiltered page.
    /// </summary>
    /// <param name="options">Paging, filter, and ordering. <see langword="null"/> means the unfiltered page.</param>
    /// <returns>The page.</returns>
    IPagedCollection<TEntity> Find(SearchOptions<TEntity> options);

    /// <summary>Returns a page projected to <typeparamref name="TResult"/>. This call is not cached.</summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="options">Paging, filter, and ordering.</param>
    /// <param name="projection">Selects the shape of each row.</param>
    /// <returns>The projected page.</returns>
    IPagedCollection<TResult> Find<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection);

    /// <summary>
    /// Returns a page of entities. A query on <paramref name="options"/> is read asynchronously, and its cancellation token is passed to the store.
    /// <see langword="null"/> options, or options whose query is <see langword="null"/>, return the cached unfiltered page from the synchronous find. That path does not observe a cancellation token.
    /// </summary>
    /// <param name="options">Paging, filter, and ordering. <see langword="null"/> means the unfiltered page.</param>
    /// <returns>The page.</returns>
    Task<IPagedCollection<TEntity>> FindAsync(SearchOptions<TEntity> options);

    /// <summary>Returns a page projected to <typeparamref name="TResult"/>. This call is not cached. A cancellation token on <paramref name="options"/> is passed to the store.</summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="options">Paging, filter, and ordering.</param>
    /// <param name="projection">Selects the shape of each row.</param>
    /// <returns>The projected page.</returns>
    Task<IPagedCollection<TResult>> FindAsync<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection);

    /// <summary>Returns the entity with the given key, using the cache. A missing key part is stored in the cache key as the text null.</summary>
    /// <param name="keyValues">The primary-key values, in order.</param>
    /// <returns>The entity, or <see langword="null"/> when the store has none.</returns>
    TEntity FindOne(params object[] keyValues);

    /// <summary>Returns the first entity matching <paramref name="options"/>. This call is not cached.</summary>
    /// <param name="options">The filter and ordering.</param>
    /// <returns>The entity, or <see langword="null"/> when none match.</returns>
    TEntity FindOne(SearchOptions<TEntity> options);

    /// <summary>Returns the first projected row matching <paramref name="options"/>. This call is not cached.</summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="options">The filter and ordering.</param>
    /// <param name="projection">Selects the shape of the row.</param>
    /// <returns>The projected value, or the default when none match.</returns>
    TResult FindOne<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection);

    /// <summary>Returns the entity with the given key from the cache. The store is read synchronously on a miss, and the task is already completed.</summary>
    /// <param name="keyValues">The primary-key values, in order. A missing part is stored in the cache key as the text null.</param>
    /// <returns>The entity, or <see langword="null"/> when the store has none.</returns>
    Task<TEntity> FindOneAsync(params object[] keyValues);

    /// <summary>Returns the first entity matching <paramref name="options"/>. This call is not cached. A cancellation token on <paramref name="options"/> is passed to the store.</summary>
    /// <param name="options">The filter and ordering.</param>
    /// <returns>The entity, or <see langword="null"/> when none match.</returns>
    Task<TEntity> FindOneAsync(SearchOptions<TEntity> options);

    /// <summary>Returns the first projected row matching <paramref name="options"/>. This call is not cached. A cancellation token on <paramref name="options"/> is passed to the store.</summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="options">The filter and ordering.</param>
    /// <param name="projection">Selects the shape of the row.</param>
    /// <returns>The projected value, or the default when none match.</returns>
    Task<TResult> FindOneAsync<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection);

    #endregion Find

    #region Count

    /// <summary>Counts every row. This call is not cached.</summary>
    /// <returns>The row count.</returns>
    int Count();

    /// <summary>Counts rows that match <paramref name="countExpression"/>. This call is not cached.</summary>
    /// <param name="countExpression">The filter.</param>
    /// <returns>The row count.</returns>
    int Count(Expression<Func<TEntity, bool>> countExpression);

    /// <summary>Counts every row. This call is not cached.</summary>
    /// <returns>The row count.</returns>
    Task<int> CountAsync();

    /// <summary>Counts rows that match <paramref name="countExpression"/>. This call is not cached.</summary>
    /// <param name="countExpression">The filter.</param>
    /// <returns>The row count.</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> countExpression);

    #endregion Count

    #region Delete

    /// <summary>Deletes every row and drops the cache for this entity.</summary>
    /// <returns>The number of rows the store reported.</returns>
    int DeleteAll();

    /// <summary>Deletes <paramref name="entity"/> and drops the cache for this entity.</summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>The number of rows the store reported.</returns>
    int Delete(TEntity entity);

    /// <summary>Deletes <paramref name="entities"/> and drops the cache for this entity.</summary>
    /// <param name="entities">The entities to delete.</param>
    /// <returns>The number of rows the store reported.</returns>
    int Delete(IEnumerable<TEntity> entities);

    /// <summary>Deletes rows that match <paramref name="filterExpression"/> and drops the cache for this entity.</summary>
    /// <param name="filterExpression">The filter.</param>
    /// <returns>The number of rows the store reported.</returns>
    int Delete(Expression<Func<TEntity, bool>> filterExpression);

    /// <summary>Deletes the rows in <paramref name="query"/> and drops the cache for this entity.</summary>
    /// <param name="query">The rows to delete.</param>
    /// <returns>The number of rows the store reported.</returns>
    int Delete(IQueryable<TEntity> query);

    /// <summary>Deletes every row and drops the cache for this entity.</summary>
    /// <returns>The number of rows the store reported.</returns>
    Task<int> DeleteAllAsync();

    /// <summary>Deletes <paramref name="entity"/> and drops the cache for this entity.</summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>The number of rows the store reported.</returns>
    Task<int> DeleteAsync(TEntity entity);

    /// <summary>Deletes <paramref name="entities"/> and drops the cache for this entity.</summary>
    /// <param name="entities">The entities to delete.</param>
    /// <returns>The number of rows the store reported.</returns>
    Task<int> DeleteAsync(IEnumerable<TEntity> entities);

    /// <summary>Deletes rows that match <paramref name="filterExpression"/> and drops the cache for this entity.</summary>
    /// <param name="filterExpression">The filter.</param>
    /// <returns>The number of rows the store reported.</returns>
    Task<int> DeleteAsync(Expression<Func<TEntity, bool>> filterExpression);

    /// <summary>Deletes the rows in <paramref name="query"/> and drops the cache for this entity.</summary>
    /// <param name="query">The rows to delete.</param>
    /// <returns>The number of rows the store reported.</returns>
    Task<int> DeleteAsync(IQueryable<TEntity> query);

    #endregion Delete

    #region Insert

    /// <summary>Inserts <paramref name="entity"/> and drops the cache for this entity.</summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The entity the store returned.</returns>
    TEntity Insert(TEntity entity);

    /// <summary>Inserts <paramref name="entities"/> and drops the cache for this entity.</summary>
    /// <param name="entities">The entities to insert.</param>
    /// <returns>The entities the store returned.</returns>
    IEnumerable<TEntity> Insert(IEnumerable<TEntity> entities);

    /// <summary>Inserts <paramref name="entity"/> and drops the cache for this entity.</summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The entity the store returned.</returns>
    Task<TEntity> InsertAsync(TEntity entity);

    /// <summary>Inserts <paramref name="entities"/> and drops the cache for this entity.</summary>
    /// <param name="entities">The entities to insert.</param>
    /// <returns>The entities the store returned.</returns>
    Task<IEnumerable<TEntity>> InsertAsync(IEnumerable<TEntity> entities);

    #endregion Insert

    #region Update

    /// <summary>Updates <paramref name="entity"/> and drops the cache for this entity.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The entity the store returned.</returns>
    TEntity Update(TEntity entity);

    /// <summary>Updates <paramref name="entities"/> and drops the cache for this entity.</summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>The entities the store returned.</returns>
    IEnumerable<TEntity> Update(IEnumerable<TEntity> entities);

    /// <summary>Updates <paramref name="entity"/> and drops the cache for this entity.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The entity the store returned.</returns>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>Updates <paramref name="entities"/> and drops the cache for this entity.</summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>The entities the store returned.</returns>
    Task<IEnumerable<TEntity>> UpdateAsync(IEnumerable<TEntity> entities);

    #endregion Update
}
