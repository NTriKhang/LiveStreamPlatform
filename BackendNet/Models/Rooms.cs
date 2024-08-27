using BackendNet.Models.Submodel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class Rooms
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { set; get; }
        public string RoomKey { get; set; }
        public string RoomTitle { get; set; }
        public string RoomThumbnail { set; get; }
        public int Status { get; set; }
        public int Mode { get; set; }
        public DateTime CDate { set; get; }
        public SubUser Owner { set; get; }
        public List<SubUser> Attendees { set; get; }
        //public Videos Video { get; set; }
    }
}
