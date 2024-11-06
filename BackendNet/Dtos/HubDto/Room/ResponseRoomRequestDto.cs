namespace BackendNet.Dtos.HubDto.Room
{
    public class ResponseRoomRequestDto : ModelHub
    {
        public string RoomId { get; set; }
        public int? RoomType { get; set; } = null;
        public string StudentId { get; set; }
        public bool Res { get; set; }

    }
}
