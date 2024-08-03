using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models.Submodel
{
    public class SubUser
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string user_id { set; get; }
        public string user_name { set; get; }
        public string user_avatar { set; get; }

        public SubUser()
        {
            user_id = string.Empty;
            user_name = string.Empty;
            user_avatar = string.Empty;
        }
        public SubUser(string user_id, string user_name, string user_avatar)
        {
            this.user_name = user_name;
            this.user_avatar = user_avatar;
            this.user_id = user_id;
        }
    }
}
