using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BackendNet.Repository.IRepositories
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> Add(TEntity obj);
        Task<TEntity> GetByKey(string key, string keyValue);
        Task<TEntity> GetByKey(string key, string keyValue, ProjectionDefinition<TEntity> projectionDefinition);
        Task<IEnumerable<TEntity>> GetMany(int page, int size);
        Task<IEnumerable<TEntity>> GetMany(int page, int size, FilterDefinition<TEntity>? additionalFilter);
        Task<IEnumerable<TEntity>> GetMany(int page, int size, FilterDefinition<TEntity>? additionalFilter, SortDefinition<TEntity>? sorDef);
        Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, FilterDefinition<TEntity>? additionalFilter = null);   
        Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, int page, int size , FilterDefinition<TEntity>? additionalFilter = null);
        Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, int page , int size, FilterDefinition<TEntity>? additionalFilter = null, SortDefinition<TEntity>? sorDef = null);
        Task<IEnumerable<TEntity>> GetAll();
        Task<UpdateResult> UpdateByKey(string key, string keyValue, UpdateDefinition<TEntity> updateDefinition);
        Task<ReplaceOneResult> ReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity);
        Task<bool> RemoveByKey(string key, string keyValue);
        Task<bool> IsExist(FilterDefinition<TEntity> filter);
        Task<IEnumerable<BsonDocument>> ExecAggre(BsonDocument[] pipeline);
        Task<IEnumerable<TEntity>> ExecAggre(PipelineDefinition<TEntity,TEntity> pipeline);

    }
}
