using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class SysRelationship
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [BsonId]
        public string? Id { set; get; }  
        public string FromCollection { set; get; }
        public string FromKey { set; get; } 
        public List<string> FromField { set;get; }
        public string ToCollection { set; get; }
        public string ToKey { set; get; }
        public List<string> ToField { set; get; }
    }
}
