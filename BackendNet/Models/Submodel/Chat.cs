using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Models.Submodel
{
    public class Chat
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string userId { get; set; }
        public string userName { get; set; }
        public string userAvatar {  get; set; }
        public DateTime? createdAt { get; set; }
        public string content { get; set; }
        public Chat()
        {
            
        }
        public Chat(string userId, DateTime dateTime, string content, string userName, string userAvatar)
        {
            this.userId = userId;
            this.createdAt = dateTime;
            this.content = content;
            this.userName = userName;
            this.userAvatar = userAvatar;

        }
    }
}
