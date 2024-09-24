namespace BackendNet.Dtos.Redis
{
    public class RecommendModel
    {
        public string user_id { get; set; }
        public string item_id { get; set; }
        public string predicted_rating { get; set; }
    }
}
