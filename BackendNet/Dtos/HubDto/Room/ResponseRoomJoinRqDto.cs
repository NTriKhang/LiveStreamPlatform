namespace BackendNet.Dtos.HubDto.Room
{
    public class ResponseRoomJoinRqDto
    {
        public string user_id { set; get; }
        public string user_name { set; get; }
        public string user_avatar { set; get; }
        public string room_id { set; get; }
        public ResponseRoomJoinRqDto(string user_id, string user_name, string user_avatar, string room_id)
        {
            this.user_id = user_id;
            this.user_name = user_name;
            this.user_avatar = user_avatar;
            this.room_id = room_id;
        }
    }
}
