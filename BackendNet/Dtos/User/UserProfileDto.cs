using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Dtos.User
{
    public class UserProfileDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; } = "";
        public string DislayName { get; set; } = "";
        public string Role { get; set; } = "";
        public string AvatarUrl { set; get; } = "";
        public UserProfileDto(string? id, string userName, string email, string dislayName, string role, string avatarUrl)
        {
            Id = id;
            UserName = userName;
            Email = email;
            DislayName = dislayName;
            Role = role;
            AvatarUrl = avatarUrl;
        }
    }
}
