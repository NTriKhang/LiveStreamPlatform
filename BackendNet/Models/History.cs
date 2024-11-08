using BackendNet.Models.Submodel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class History
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        public string Id { get; set; }
        public SubVideo Video { get; set; }
        public SubUser User { set; get; }
        public int PlayTime { get; set; }
        public DateTime Time { set; get; }
        public bool Like { set; get; }
        public bool Dislike { set; get; }
        public int Comment { set; get; }
        public int? calculated_rating { set; get; }
        public History()
        {
            Video = new SubVideo();
            User = new SubUser();
            PlayTime = 0;
            Time = DateTime.UtcNow;
            Like = false;
            Dislike = false;
            Comment = 0;
        }
        public History(
            SubVideo video
            , SubUser user
            , int playTime
            , DateTime time
            , bool like
            , bool dislike
            , int comment)
        {
            Video = video;
            User = user;
            PlayTime = playTime;
            Time = time;
            Like = like;
            Dislike = dislike;
            Comment = comment;
        }

    }
}
