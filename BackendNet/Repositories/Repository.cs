using BackendNet.DAL;
using BackendNet.Repository.IRepositories;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
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

        #region add method
        public virtual async Task<TEntity> Add(TEntity obj)
        {
            await _collection.InsertOneAsync(obj);
            return obj;
        }
        #endregion

        #region get many
        public async Task<IEnumerable<TEntity>> GetAll()
        {
            var all = await _collection.FindAsync(Builders<TEntity>.Filter.Empty);
            return all.ToList();
        }
        public async Task<IEnumerable<TEntity>> GetMany(int page, int size)
        {
            var all = _collection.Find(Builders<TEntity>.Filter.Empty).Skip(size * (page - 1)).Limit(size);
            return await all.ToListAsync();
        }
        public Task<IEnumerable<TEntity>> GetMany(int page, int size, FilterDefinition<TEntity>? additionalFilter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TEntity>> GetMany(int page, int size, FilterDefinition<TEntity>? additionalFilter, SortDefinition<TEntity>? sorDef)
        {
            IEnumerable<TEntity> data;
            var filter = Builders<TEntity>.Filter.Empty;

            if (additionalFilter != null)
                filter &= additionalFilter;


            data = await _collection.Find(filter).Sort(sorDef).Skip(size * (page - 1)).Limit(size).ToListAsync();

            return data;
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

        public async Task<IEnumerable<TEntity>> GetManyByKey(string key, string keyValue, int page, int size, FilterDefinition<TEntity>? additionalFilter = null, SortDefinition<TEntity>? sorDef = null)
        {

            IEnumerable<TEntity> data;
            var filter = FilterId(key, keyValue);

            if (additionalFilter != null)
                filter &= additionalFilter;


            data = await _collection.Find(filter).Sort(sorDef).Skip(size * (page - 1)).Limit(size).ToListAsync();

            return data;
        }
        #endregion

        #region get one
        public virtual async Task<TEntity> GetByKey(string key, string id)
        {
            var data = await _collection.FindAsync(FilterId(key, id));
            var res = data.SingleOrDefault();
            return res;
        }
        public virtual async Task<TEntity> GetByKey(string key, string id, ProjectionDefinition<TEntity> projectionDefinition)
        {
            var data = await _collection.Find(FilterId(key, id))
                                            .Project<TEntity>(projectionDefinition)
                                            .ToListAsync();
            var res = data.SingleOrDefault();  
            return res;
        }

        #endregion

        #region aggregrate
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

        #endregion

        #region Update method
        public async Task<UpdateResult> UpdateByKey(string key, string id, UpdateDefinition<TEntity> updateDefinition)
        {
            return await _collection.UpdateOneAsync(FilterId(key, id), updateDefinition);

        }
        public async Task<ReplaceOneResult> ReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity)
        {
            return await _collection.ReplaceOneAsync(filter, entity);

        }
        #endregion



        public virtual async Task<bool> RemoveByKey(string key, string id)
        {
            var result = await _collection.DeleteOneAsync(FilterId(key, id));
            return result.IsAcknowledged;
        }

     
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        protected static FilterDefinition<TEntity> FilterId(string key, string keyValue)
        {
            return Builders<TEntity>.Filter.Eq(key, keyValue);
        }
        

        public async Task<bool> IsExist(FilterDefinition<TEntity>? filter)
        {
            return await _collection.CountDocumentsAsync(filter) > 0;
        }

      

    }
}
