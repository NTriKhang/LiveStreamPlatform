using System.Security.Cryptography;
using System.Text;

namespace BackendNet
{
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
        Student
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
        Init,
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
}
