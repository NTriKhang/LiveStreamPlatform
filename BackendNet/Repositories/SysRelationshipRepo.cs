using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BackendNet.Repositories
{
    public class SysRelationshipRepo : Repository<SysRelationship>, ISysRelationshipRepo
    {
        public SysRelationshipRepo(IMongoContext context) : base(context)
        {
            
        }
        public async Task UpdateUserMonitor(Users user)
        {
            var filterRelate = Builders<SysRelationship>.Filter.Eq(x => x.FromCollection, nameof(Users));
            var listRelate = await GetAll(filterRelate, null);

            foreach (var relate in listRelate)
            {
                var targetCollection = _database.GetCollection<BsonDocument>(relate.ToCollection);
                var dynamicFilter = Builders<BsonDocument>.Filter.Eq(relate.ToKey, ObjectId.Parse(user.Id));
                var updates = new List<UpdateDefinition<BsonDocument>>();

                var sampleDocument = await targetCollection.Find(dynamicFilter).FirstOrDefaultAsync();
                if (sampleDocument == null) continue;

                var isArray = relate.ToField.Any(toField =>
                {
                    var fieldName = toField.Split('.')[0];
                    return sampleDocument.Contains(fieldName) && sampleDocument[fieldName].IsBsonArray;
                });

                if (!isArray)
                {
                    for (int i = 0; i < relate.FromField.Count; i++)
                    {
                        string fromField = relate.FromField[i];
                        string toField = relate.ToField[i];

                        var updatedValue = user.GetType().GetProperty(fromField)?.GetValue(user);
                        if (updatedValue != null)
                        {
                            updates.Add(Builders<BsonDocument>.Update.Set(toField, updatedValue));
                        }
                    }

                    if (updates.Count > 0)
                    {
                        var updateDefinition = Builders<BsonDocument>.Update.Combine(updates);
                        await targetCollection.UpdateManyAsync(dynamicFilter, updateDefinition);
                    }
                }
                else
                {
                    var arrayFilter = new List<ArrayFilterDefinition<BsonDocument>>
                        {
                            new BsonDocumentArrayFilterDefinition<BsonDocument>(
                                new BsonDocument($"elem.{relate.ToKey.Split('.')[1]}", ObjectId.Parse(user.Id))
                            )
                        };

                    for (int i = 0; i < relate.FromField.Count; i++)
                    {
                        var fieldParts = relate.ToField[i].Split('.');
                        var arrayField = fieldParts[0]; 
                        var nestedField = string.Join(".", fieldParts.Skip(1)); 


                        string fromField = relate.FromField[i];
                        string toFieldNested = $"{arrayField}.$[elem].{nestedField}";

                        var updatedValue = user.GetType().GetProperty(fromField)?.GetValue(user);
                        if (updatedValue != null)
                        {
                            updates.Add(Builders<BsonDocument>.Update.Set(toFieldNested, updatedValue));
                        }
                    }
                    if (updates.Count > 0)
                    {
                        var updateDefinition = Builders<BsonDocument>.Update.Combine(updates);
                        await targetCollection.UpdateManyAsync(
                            dynamicFilter,
                            updateDefinition,
                            new UpdateOptions { ArrayFilters = arrayFilter }
                        );
                    }
                }
            }
        }
    }
}
