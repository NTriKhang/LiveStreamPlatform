using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections;

namespace BackendNet.Models
{
    public class Videos
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string User_id { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? Time { set; get; }
        public string Title { set; get; }
        public string? Description { set; get; }
        public int? View { set; get; }
        public int? Like { set; get; }
        public string Thumbnail { set; get; }
        public string? Status { set; get; }
        public int? StatusNum { set; get; }

        public long VideoSize { set; get; }
        public string FileType { set; get; }
        public List<string> Tags { set; get; }
    }
}
