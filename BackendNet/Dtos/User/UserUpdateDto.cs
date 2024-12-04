using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Dtos.User
{
    public class UserUpdateDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Email { get; set; } = "";
        public string DislayName { get; set; } = "";
        public string AvatarUrl { set; get; } = "";
    }
}
