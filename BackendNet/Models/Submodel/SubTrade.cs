using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models.Submodel
{
    public class SubTrade
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string CourseId { set; get; }
        public string CourseTitle {  set; get; }
        public string CourseImage { set; get; }
        public decimal Price { set; get; }
        public DateTime DateIncome { set; get; }
    }
}
