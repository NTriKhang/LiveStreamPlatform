namespace BackendNet.Dtos.HubDto
{
    public class ResponseRoomRequest : ModelHub
    {
        public string RoomId { get; set; }
        public string StudentId { get; set; }
        public bool Res {  get; set; }

    }
}
