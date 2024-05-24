using BackendNet.Models.Submodel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public StreamInfo StreamInfo { get; set; }

    }
}
