using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using BackendNet.Models.Submodel;

namespace BackendNet.Models
{
    public class Trending
    {
        /// <summary>
        /// Video Id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 
        public int TotalView { get; set; }
        /// <summary>
        /// cập nhật lại mỗi khi có lượt xem video
        /// </summary>
        public DateTime NewestViewAt { get; set; }
        public double GrowthRate { get; set; }
        public List<ViewTrackHistory> ViewTrackHistories { get; set; }
    }
}
