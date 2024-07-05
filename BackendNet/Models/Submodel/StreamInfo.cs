using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BackendNet.Models.Submodel
{
    public class StreamInfo
    {
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? Last_stream { set; get; }
        public string? Stream_token { get; set; }
        public string? Status { get; set; }
        public StreamInfo() 
        {
            Stream_token = null;
        }
        public StreamInfo(DateTime? last_stream, string stream_token, string status)
        {
            Last_stream = last_stream;
            Stream_token = stream_token;
            Status = status;
        }
    }
}
