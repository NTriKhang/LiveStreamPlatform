using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using BackendNet.Models.Submodel;

namespace BackendNet.Models
{
    public class ChatLive
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { set; get; } = string.Empty;
        [BsonRepresentation(BsonType.ObjectId)]
        public string room_id { set; get; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string userId { get; set; }
        public string userName { get; set; }
        public string userAvatar { get; set; }
        public DateTime? createdAt { get; set; }
        public string content { get; set; }

    }
}
