namespace BackendNet.Dtos.Room
{
    public class RoomCreateDto
    {
        public string RoomKey { get; set; }
        public string RoomTitle { get; set; }
        public string RoomThumbnail { set; get; }
        public int RoomType { set; get; }
    }
}
