using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using BackendNet.Models.Submodel;

namespace BackendNet.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { set; get; }
        public string Title { set; get; }
        public string Desc { set; get; }
        public string CourseDetail { set; get; }
        public decimal Price { set; get; }
        public List<string> Tags { set; get; }
        public decimal Discount { set; get; }
        public SubUser Cuser { set; get; }
        public DateTime Cdate { set; get; }
        public DateTime Edate { set; get; }
        public List<CourseStudent> Students { set; get; }
    }
}
