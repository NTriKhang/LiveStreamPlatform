using MongoDB.Driver;

namespace BackendNet.Repository.IRepositories
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> Add(TEntity obj);
        Task<TEntity> GetByKey(string key, string keyValue);
        Task<List<TEntity>> GetManyByKey(string key, string keyValue, int? page =null, FilterDefinition<TEntity>? additionalFilter = null);
        Task<IEnumerable<TEntity>> GetAll();
        Task<UpdateResult> UpdateByKey(string key, string keyValue, UpdateDefinition<TEntity> updateDefinition);
        Task<bool> RemoveByKey(string key, string keyValue);
    }
}
