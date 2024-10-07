using BackendNet.Setting;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BackendNet.Repository.IRepositories
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> Add(TEntity obj);
        Task<TEntity> GetByFilter(FilterDefinition<TEntity> filter);
        Task<PaginationModel<TEntity>> GetManyByFilter(int page, int pageSize, FilterDefinition<TEntity> filter, SortDefinition<TEntity> sorDef);
        Task<PaginationModel<TEntity>> GetManyByFilter(int page, int pageSize, FilterDefinition<TEntity> filter, SortDefinition<TEntity> sorDef, ProjectionDefinition<TEntity> projDef);
        Task<IEnumerable<TEntity>> GetMany(int page, int size);
        Task<IEnumerable<TEntity>> GetMany(int page, int size, FilterDefinition<TEntity>? additionalFilter);
        Task<PaginationModel<TEntity>> GetMany(int page, int size, FilterDefinition<TEntity>? additionalFilter, SortDefinition<TEntity>? sorDef);
        Task<UpdateResult> UpdateByFilter(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition);
        Task<ReplaceOneResult> ReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity);
        Task<bool> RemoveByKey(string key, string keyValue);
        Task<bool> RemoveByKey(string key, string id, DeleteOptions deleteOptions);
        Task<bool> IsExist(FilterDefinition<TEntity> filter);
        Task<IEnumerable<BsonDocument>> ExecAggre(BsonDocument[] pipeline);
        Task<IEnumerable<TEntity>> ExecAggre(PipelineDefinition<TEntity,TEntity> pipeline);
        Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update);

        // old version
        Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, FilterDefinition<TEntity>? additionalFilter = null);
        Task<PaginationModel<TEntity>> GetManyByKey(string key, string keyValue, int page, int size, FilterDefinition<TEntity>? additionalFilter = null);
        Task<IEnumerable<TEntity>> GetAll();

    }
}
