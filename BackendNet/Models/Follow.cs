using BackendNet.Models.Submodel;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Models
{
    public class Follow
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public FollowInfo Followed { get; set; } 
        public FollowInfo Follower { get; set; }
        public DateTime FollowDate { get; set; }
        public Follow()
        {
            Followed = new FollowInfo();
            Follower = new FollowInfo();
            FollowDate = DateTime.Now;
        }
        public Follow(FollowInfo Followed, FollowInfo Follower, DateTime FollowDate)
        {
            this.Followed = Followed;
            this.Follower = Follower;
            this.FollowDate = FollowDate;
        }
    }
}
