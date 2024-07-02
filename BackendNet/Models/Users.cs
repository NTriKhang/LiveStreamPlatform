using BackendNet.Models.Submodel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; } = "";
        public bool IsEmailActive {get; set;} = false;
        public string DislayName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role {get;set;} = "";
        public string AvatarUrl { set; get; } = "";
        public StreamInfo StreamInfo { get; set; }

    }
}
