using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class Rooms
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { set; get; }
        public string RoomKey { get; set; }
        public string StreamKey { get; set; }
        public string Status { get; set; }
        public Videos Video { get; set; }
    }
}
