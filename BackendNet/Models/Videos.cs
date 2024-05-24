using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class Videos
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string User_id { get; set; }
        public string Title { set; get; }
        public string? Description { set; get; }
        public int? View { set; get; }
        public int? Like { set; get; }
        public string Thumbnail { set; get; }
        public string? Status { set; get; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? Time { set; get; }
        public string Video_token { set; get; }
    }
}
