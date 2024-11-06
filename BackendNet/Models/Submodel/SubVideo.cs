using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models.Submodel
{
    public class SubVideo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string video_id { set; get; }
        public string video_title { set; get; }
        public string video_thumb { set; get; }
        public DateTime video_date { set; get; }
        public List<string> tags { set; get; }

        public SubVideo()
        {
            video_id = string.Empty;
            video_title = string.Empty;
            video_thumb = string.Empty;
            tags = new List<string>();
            video_date = DateTime.UtcNow;
        }
        public SubVideo(string video_id
            , string video_title
            , string video_thumb
            , List<string> tags
            , DateTime video_date)
        {
            this.video_title = video_title;
            this.video_thumb = video_thumb;
            this.video_id = video_id;
            this.tags = tags;   
            this.video_date = video_date;
        }
    }
}
