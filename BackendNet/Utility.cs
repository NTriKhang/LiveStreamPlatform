using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace BackendNet
{
    struct SessionInfo
    {
        public string UserId { set; get; }
        public DateTime ExpiredTime { set;get; }
    }
    public static class AuthSession
    {
        private static List<SessionInfo> Sessions = new List<SessionInfo>();
        public static bool IsExist(string UserId)
        {
            return Sessions.Any(x => x.UserId == UserId);   
        }
        public static void RemoveUserId(string UserId)
        {
            var removed = Sessions.Where(x => x.UserId == UserId);
            if (removed.Any())
            {
                Sessions.Remove(removed.SingleOrDefault());
            }
        }
        public static bool AddUserId(string UserId, DateTime ExpiredTime)
        {
            if (Sessions.Any(x => x.UserId == UserId) 
                && Sessions.Where(x => x.UserId == UserId).Select(x => x.ExpiredTime).Single() > DateTime.UtcNow)
                return false;

            var removed = Sessions.Where(x => x.UserId == UserId);
            if (removed.Any())
            {
                Sessions.Remove(removed.SingleOrDefault());
            }

            Sessions.Add(new SessionInfo {  UserId = UserId, ExpiredTime = ExpiredTime });
            return true;
        }
    }
    public static class Utility
    {
        public const string ThumbnailImage = "Thumbnail";
        public const string UserId = "UserId";
        public const string SplitKeyStream = "_";
        public const int ItemsPerPage = 6;
        public static string GenerateStreamKey(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var stringBuilder = new StringBuilder(length);

            foreach (var byteValue in randomBytes)
            {
                stringBuilder.Append(validChars[byteValue % validChars.Length]);
            }

            return stringBuilder.ToString();
        }
    }
    public enum ConfigKey
    {
        CloudFrontEduVideo
    }
    public enum RoleKey
    {
        Teacher,
        Student,
        Admin
    }
    public enum PaginationCount
    {
        Chat = 10,
        Video = 6,
        Follow = 25,
        Course = 25,
    }
    public enum StreamStatus
    {
        Idle,
        Streaming
    }
    public enum VideoStatus
    {
        Public, //0
        Private,//1
        Keep,//2
        Member,//3
        Remove,//4
        Upload,//5
        TestData//6
    }
    public enum RoomStatus
    {
        Opening,
        Closed,
        Expired,
    }
    public enum RoomType
    {
        Meeting,
        LiveStream
    }
    public enum Mode
    {
        Public,
        Private
    }
}
