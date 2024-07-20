using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models.Submodel
{
    public class FollowInfo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string user_id { set; get; } 
        public string user_display_name { set; get; }   
        public string user_avatar { set; get; }

        public FollowInfo()
        {
            user_id = string.Empty;
            user_display_name = string.Empty;
            user_avatar = string.Empty;
        }
        public FollowInfo(string user_id, string user_display_name, string user_avatar)
        {
            this.user_id = user_id;
            this.user_display_name= user_display_name;
            this.user_avatar = user_avatar;
        }
    }
}
