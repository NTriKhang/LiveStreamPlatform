using MongoDB.Driver;
using System.Linq.Expressions;

namespace BackendNet.Repository.IRepositories
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> Add(TEntity obj);
        Task<TEntity> GetByKey(string key, string keyValue);
        Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, FilterDefinition<TEntity>? additionalFilter = null);   
        Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, int page, int size , FilterDefinition<TEntity>? additionalFilter = null);
        Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, int page , int size, bool isSort = false, SortDefinition<TEntity>? sorDef = null, FilterDefinition<TEntity>? additionalFilter = null);
        Task<IEnumerable<TEntity>> GetAll();
        Task<UpdateResult> UpdateByKey(string key, string keyValue, UpdateDefinition<TEntity> updateDefinition);
        Task<bool> RemoveByKey(string key, string keyValue);
    }
}
