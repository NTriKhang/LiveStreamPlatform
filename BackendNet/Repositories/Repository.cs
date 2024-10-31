using Amazon.S3.Model;
using BackendNet.DAL;
using BackendNet.Repository.IRepositories;
using BackendNet.Setting;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Security.Policy;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackendNet.Repository
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IMongoDatabase _database;
        protected readonly IMongoClient _client;
        protected readonly IMongoCollection<TEntity> _collection;
        public Repository(IMongoContext context)
        {
            _database = context.Database;
            _client = context.Client;
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
        public async Task<IEnumerable<TEntity>> GetAll(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sortDefinition)
        {
            var all = _collection.Find(filter).Sort(sortDefinition);
            return await all.ToListAsync();
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

        public async Task<PaginationModel<TEntity>> GetMany(int page, int size, FilterDefinition<TEntity>? additionalFilter, SortDefinition<TEntity>? sorDef)
        {
            IEnumerable<TEntity> data;
            var filter = Builders<TEntity>.Filter.Empty;

            if (additionalFilter != null)
                filter &= additionalFilter;


            data = await _collection.Find(filter).Sort(sorDef).Skip(size * (page - 1)).Limit(size).ToListAsync();
            var model = new PaginationModel<TEntity>();
            model.data = data.ToList();
            model.page = page;
            model.pageSize = size;
            model.total_rows = (int)(await _collection.Find(filter).CountDocumentsAsync());
            model.total_pages = (int)Math.Ceiling(model.total_rows / (double)size);

            return model;
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

        public async Task<PaginationModel<TEntity>> GetManyByKey(string key, string keyValue, int page, int size, FilterDefinition<TEntity>? additionalFilter = null)
        {
            var filter = FilterId(key, keyValue);

            if (additionalFilter != null)
                filter &= additionalFilter;

            var data = await _collection.FindAsync(filter);

            var model = new PaginationModel<TEntity>();
            model.data = data.ToList();
            model.page = page;
            model.pageSize = size;
            model.total_rows = (int)(await _collection.Find(filter).CountDocumentsAsync());
            model.total_pages = (int)Math.Ceiling(model.total_rows / (double)size);

            return model;
        }
        #endregion

        #region get one

        public async Task<TEntity> GetByFilter(FilterDefinition<TEntity> Filter)
        {
            var data = await _collection.FindAsync(Filter);
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
        public async Task<ReplaceOneResult> ReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity)
        {
            return await _collection.ReplaceOneAsync(filter, entity);

        }
        #endregion

        #region delete method
        public virtual async Task<bool> RemoveByKey(string key, string id)
        {
            var result = await _collection.DeleteOneAsync(FilterId(key, id));
            return result.DeletedCount > 0;
        }
        public virtual async Task<bool> RemoveByKey(string key, string id, DeleteOptions deleteOptions)
        {
            var res = await _collection.DeleteOneAsync(FilterId(key, id), deleteOptions);
            return res.DeletedCount > 0;
        }
        #endregion

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

        public async Task<UpdateResult> UpdateByFilter(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition)
        {
            return await _collection.UpdateOneAsync(filterDefinition, updateDefinition);
        }

        public async Task<PaginationModel<TEntity>> GetManyByFilter(int page, int pageSize, FilterDefinition<TEntity> filter, SortDefinition<TEntity> sorDef)
        {
            var data = await _collection.Find(filter)
                .Sort(sorDef)
                .Skip(pageSize * (page - 1))
                .Limit(pageSize)
                .ToListAsync();
            var model = new PaginationModel<TEntity>();
            if (data.Count == 0)
            {
                model.data = null;
                model.page = page;
                model.pageSize = data.Count;
                model.total_rows = 0;
                model.total_pages = 0;
                return model;
            }
            model.data = data.ToList();
            model.page = page;
            model.pageSize = data.Count;
            model.total_rows = (int)(await _collection.Find(filter).CountDocumentsAsync());
            model.total_pages = (int)Math.Ceiling(model.total_rows / (double)pageSize);

            return model;
        }
        public async Task<PaginationModel<TEntity>> GetManyByFilter(int page, int pageSize, FilterDefinition<TEntity> filter, SortDefinition<TEntity> sorDef, ProjectionDefinition<TEntity> projDef)
        {
            var data = await _collection.Find(filter)
                .Sort(sorDef)
                .Project<TEntity>(projDef)
                .Skip(pageSize * (page - 1))
                .Limit(pageSize)
                .ToListAsync();
            var model = new PaginationModel<TEntity>();
            if (data.Count == 0)
            {
                model.data = null;
                model.page = page;
                model.pageSize = data.Count;
                model.total_rows = 0;
                model.total_pages = 0;
                return model;
            }
            model.data = data.ToList();
            model.page = page;
            model.pageSize = data.Count;
            model.total_rows = (int)(await _collection.Find(filter).CountDocumentsAsync());
            model.total_pages = (int)Math.Ceiling(model.total_rows / (double)pageSize);

            return model;
        }
        public async Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update)
        {
            return await _collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<TEntity>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            }); ;
        }
    }
}
