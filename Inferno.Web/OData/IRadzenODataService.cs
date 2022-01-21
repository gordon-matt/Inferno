using Radzen;

namespace Inferno.Web.OData
{
    public interface IRadzenODataService<TEntity, TKey> : IDisposable
        where TEntity : class
    {
        Task<ODataServiceResult<TEntity>> FindAsync(
            string filter = default,
            int? top = default,
            int? skip = default,
            string orderby = default,
            string expand = default,
            string select = default,
            bool? count = default);

        Task<TEntity> FindOneAsync(TKey key);

        Task<TEntity> InsertAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TKey key, TEntity entity);

        Task<HttpResponseMessage> DeleteAsync(TKey key);
    }
}