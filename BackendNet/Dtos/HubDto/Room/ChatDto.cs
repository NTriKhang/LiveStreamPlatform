namespace BackendNet.Dtos.HubDto.Room
{
    public class ChatDto
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string userAvatar { get; set; }
        public string content { get; set; }
        public string room_id { set; get; }

    }
}
