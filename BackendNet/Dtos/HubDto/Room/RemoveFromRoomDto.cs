namespace BackendNet.Dtos.HubDto.Room
{
    public class RemoveFromRoomDto : ModelHub
    {
        public string RoomId { get; set; }
        public string UserId { get; set; }
    }
}
