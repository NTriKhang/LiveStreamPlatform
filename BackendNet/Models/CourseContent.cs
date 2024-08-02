using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using BackendNet.Models.Submodel;

namespace BackendNet.Models
{
    public class CourseContent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Course_id { get; set; }
        public List<Section> Sections { get; set; }
    }
}
