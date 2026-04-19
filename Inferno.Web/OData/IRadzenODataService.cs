using Inferno.Web.Models;
using Radzen;

namespace Inferno.Web.OData
{
    public interface IRadzenODataService<TEntity, TKey> : IDisposable
        where TEntity : class
    {
        Task<ApiResponse<ODataServiceResult<TEntity>>> FindAsync(
            string filter = default,
            int? top = default,
            int? skip = default,
            string orderby = default,
            string expand = default,
            string select = default,
            bool? count = default);

        Task<ApiResponse<TEntity>> FindOneAsync(TKey key);

        Task<ApiResponse<TEntity>> InsertAsync(TEntity entity);

        Task<ApiResponse<TEntity>> UpdateAsync(TKey key, TEntity entity);

        Task<ApiResponse> DeleteAsync(TKey key);
    }
}