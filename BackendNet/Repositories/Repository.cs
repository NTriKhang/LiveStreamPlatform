using BackendNet.DAL;
using BackendNet.Repository.IRepositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;

namespace BackendNet.Repository
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IMongoDatabase _database;
        protected readonly IMongoCollection<TEntity> _collection;
        public Repository(IMongoContext context)
        {
            _database = context.Database;
            _collection = _database.GetCollection<TEntity>(typeof(TEntity).Name);

        }

        public virtual async Task<TEntity> Add(TEntity obj)
        {
            await _collection.InsertOneAsync(obj);
            return obj;
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            var all = await _collection.FindAsync(Builders<TEntity>.Filter.Empty);
            return all.ToList();
        }

        public virtual async Task<TEntity> GetByKey(string key, string id)
        {
            var data = await _collection.FindAsync(FilterId(key, id));
            var res = data.SingleOrDefault();
            return res;
        }
        public virtual async Task<bool> RemoveByKey(string key, string id)
        {
            var result = await _collection.DeleteOneAsync(FilterId(key, id));
            return result.IsAcknowledged;
        }

        public async Task<UpdateResult> UpdateByKey(string key, string id, UpdateDefinition<TEntity> updateDefinition)
        {
            return await _collection.UpdateOneAsync(FilterId(key, id), updateDefinition);

        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        protected static FilterDefinition<TEntity> FilterId(string key, string keyValue)
        {
            return Builders<TEntity>.Filter.Eq(key, keyValue);
        }

        public async Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, FilterDefinition<TEntity>? additionalFilter = null)
        {
            var filter = FilterId(key, keyValue);

            if (additionalFilter != null)
            {
                filter &= additionalFilter;
            }
            var res = await _collection.FindAsync(filter);

            return res.ToList();
        }

        public async Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, int page, int size, FilterDefinition<TEntity>? additionalFilter = null)
        {
            var filter = FilterId(key, keyValue);

            if (additionalFilter != null)
                filter &= additionalFilter;


            //data = await _collection.Find(filter).Skip(size * (page - 1)).Limit(size).ToListAsync();
            var data = await _collection.FindAsync(filter);

            return data.ToList();
        }

        public async Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, int page, int size, bool isSort = false, SortDefinition<TEntity>? sorDef = null, FilterDefinition<TEntity>? additionalFilter = null)
        {

            IEnumerable<TEntity> data;
            var filter = FilterId(key, keyValue);

            if (additionalFilter != null)
                filter &= additionalFilter;


            data = await _collection.Find(filter).Sort(sorDef).Skip(size * (page - 1)).Limit(size).ToListAsync();

            return data;
        }

        public async Task<bool> IsExist(FilterDefinition<TEntity>? filter)
        {
            return await _collection.CountDocumentsAsync(filter) > 0;
        }

        public virtual async Task<IEnumerable<BsonDocument>> ExecAggre(BsonDocument[] pipeline)
        {
            var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results;
        }

        public virtual async Task<IEnumerable<TEntity>> ExecAggre(PipelineDefinition<TEntity, TEntity> pipeline)
        {
            var results = await _collection.AggregateAsync(pipeline);
            return results.Current;
        }
    }
}
