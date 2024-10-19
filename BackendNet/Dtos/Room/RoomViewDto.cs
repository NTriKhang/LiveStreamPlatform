using BackendNet.Models.Submodel;

namespace BackendNet.Dtos.Room
{
    public class RoomViewDto
    {
        public string _id { set; get; }
        public string RoomKey { get; set; }
        public string RoomTitle { get; set; }
        public string RoomThumbnail { set; get; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int Mode { get; set; }
        public int RoomType { get; set; }
        public DateTime CDate { set; get; }
        public List<SubUser> Attendees { set; get; }
    }
}
