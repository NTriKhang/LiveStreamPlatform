﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendNet.Models
{
    public class Interactions
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string videoId { get; set; }
        public int playTime { get; set; }
    }
}
