using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using BackendNet.Models.Submodel;

namespace BackendNet.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { set; get; } = string.Empty;
        public string Title { set; get; } = string.Empty;
        public string Desc { set; get; } = string.Empty;
        public string CourseDetail { set; get; } = string.Empty;
        public decimal Price { set; get; }
        public List<string> Tags { set; get; }
        public decimal Discount { set; get; }
        public SubUser Cuser { set; get; }
        public DateTime Cdate { set; get; }
        public DateTime Edate { set; get; }
        public List<CourseStudent> Students { set; get; }
        public Course()
        {
            Students = new List<CourseStudent>();
            Tags = new List<string>();
            Cuser = new SubUser();
        }
    }
}
