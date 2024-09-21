using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Models
{
    public class WatchTrainModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public TrainInfoModel TrainInfoModel { get; set; }
    }
}
