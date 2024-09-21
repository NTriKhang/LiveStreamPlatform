using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Models
{
    public class WatchHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string User_id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string Video_id { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? Time { set; get; }
    }
}
