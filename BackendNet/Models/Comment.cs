using BackendNet.Models.Submodel;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { set; get; }
        public DateTime CDate { set; get; } 
        public string Content { set; get; }
        public SubUser SubUser { set; get; }
        public int Like { set;get; }
        public int Dislike { set; get; }
        //Comment xài cho cả video và course, module video hoặc course
        public string Module { set;get; }
        //giá trị id 
        public string ModuleId { set; get; }    
    }
}
