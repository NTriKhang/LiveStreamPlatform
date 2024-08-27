namespace BackendNet.Dtos.Room
{
    public class RoomUpdateDto
    {
        public string RoomKey { get; set; }
        public string RoomTitle { get; set; }
        public string RoomThumbnail { set; get; }
        public int Mode { get; set; }
        public int Status { get; set; }
    }
}
