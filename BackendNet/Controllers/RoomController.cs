using AutoMapper;
using BackendNet.Dtos;
using BackendNet.Dtos.HubDto.Room;
using BackendNet.Dtos.Room;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System.Net;
using System.Security.Claims;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IRoomService roomService;
        private readonly IVideoService videoService;
        private readonly IUserService userService;
        private readonly IStatusService statusService;
        public RoomController(IRoomService roomService
            , IMapper mapper
            , IVideoService videoService
            , IUserService userService
            , IStatusService statusService
            )
        {
            this.mapper = mapper;
            this.roomService = roomService;
            this.videoService = videoService;
            this.userService = userService; 
            this.statusService = statusService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [HttpGet("{roomId}")]
        public async Task<ReturnModel> GetRoom(string roomId)
        {
            try
            {
                var rooms = await roomService.GetRoomById(roomId);

                if (rooms == null)
                    return new ReturnModel(400, "Không tìm thấy phòng này", roomId);

                //_ = videoService.UpdateVideoView(rooms.Video.Id!);
                if (rooms.Status == (int)RoomStatus.Closed)
                    return new ReturnModel(400, "Phòng này hiện đang đóng", roomId);
                else if (rooms.Status == (int)RoomStatus.Expired)
                    return new ReturnModel(400, "Phòng này đã hết hạn", roomId);
                return new ReturnModel(200, string.Empty, rooms);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [HttpGet("GetUserRoom")]
        public async Task<ReturnModel> GetRoom()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var res = await roomService.GetRoomByUserId(userId);
                IEnumerable<RoomViewDto> roomViewDtos = new List<RoomViewDto>();
                mapper.Map(res, roomViewDtos);

                foreach(var roomViewDto in roomViewDtos)
                {
                    var status = await statusService.GetStatus("Room", roomViewDto.Status);
                    roomViewDto.StatusName = status.Name;
                }

                return new ReturnModel(200, string.Empty, roomViewDtos);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Để ý giá trị status khi tạo, người dùng phải kết thúc phòng cũ ( status = expired ) mới được tạo phòng mới
        /// </summary>
        /// <param name="roomCreate"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReturnModel>> AddRoom(RoomCreateDto roomCreate)
        {
            try
            {
                Rooms room = new Rooms();
                mapper.Map(roomCreate,room);

                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                if (userId == string.Empty)
                    return new ReturnModel(400, "Thông tin người dùng có thể bị lỗi", null);

                var user = await userService.GetUserById(userId);

                if (user.StreamInfo != null && user.StreamInfo.Stream_token != null)
                {
                    room.CDate = DateTime.Now;
                    room.Attendees = new List<Models.Submodel.SubUser>();
                    room.RoomTitle = roomCreate.RoomTitle;
                    room.RoomThumbnail = roomCreate.RoomThumbnail;
                    room.Status = (int)RoomStatus.Opening;
                    room.Mode = (int)Mode.Public;
                    room.Owner = new Models.Submodel.SubUser(user.Id, user.DislayName, user.AvatarUrl);
                    var res = await roomService.AddRoom(room);
                    return res;
                }
                else
                {
                    return new ReturnModel((int)HttpStatusCode.TemporaryRedirect, "Cần cập nhật lại stream key của người dùng", roomCreate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [HttpDelete("{roomId}")]
        [Authorize]
        public async Task<ActionResult> DeleteRoom(string roomId)
        {
            try
            {
                var res = await roomService.DeleteRoom(roomId);
                if (res)
                    return NoContent();
                return StatusCode(StatusCodes.Status304NotModified, "Không có dữ liệu nào được xóa");
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPut]
        public async Task<ReturnModel> UpdateRoom([FromHeader] string roomId, RoomUpdateDto roomCreateDto)
        {
            try
            {
                var room = await roomService.GetRoomById(roomId);
                if (room == null)
                    return new ReturnModel(404, "Không tìm thấy phòng này", roomId);

                mapper.Map(roomCreateDto, room);
                var res = await roomService.UpdateRoom(room);
                if (res.ModifiedCount > 0)
                    return new ReturnModel(200, string.Empty, room);
                return new ReturnModel((int)HttpStatusCode.BadRequest, string.Empty, room);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Dùng khi accept yêu cầu tham gia phòng từ người dùng
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="student"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ResponseRoomRequest")]
        public async Task<ReturnModel> ResponseRoomRequest([FromBody] ResponseRoomRequestDto resRoomRequest)
        {
            try
            {
                if(resRoomRequest.Res == false)
                {
                    await roomService.ResponseRequestToStudent(resRoomRequest.RoomId, resRoomRequest.StudentId, resRoomRequest.Res, resRoomRequest.Cmd);
                    return new ReturnModel(200, string.Empty, null);
                }

                var subUser = await userService.GetSubUser(resRoomRequest.StudentId);
                var res = await roomService.AddStudentToRoom(resRoomRequest.RoomId, subUser);
                if (res)
                {
                    Task response = roomService.ResponseRequestToStudent(resRoomRequest.RoomId, resRoomRequest.StudentId, resRoomRequest.Res, resRoomRequest.Cmd);
                    _ = response;
                    return new ReturnModel(200, string.Empty, null);
                }
                return new ReturnModel(505, $"Có lỗi khi thêm {subUser.user_name} vào phòng", null);   
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// user sử dụng api này khi yêu cầu tham gia phòng học, yêu cầu sẽ được hub gửi qua cho chủ phòng
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="cmd">là tên sự kiện chủ phòng sẽ nhận, để rỗng hoặc không trùng với sự kiện ở client thì chủ phòng không nhận được yêu cầu</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("JoinRoomRequest")]
        public async Task<ReturnModel> JoinRoomRequest([FromBody] JoinRoomRequestDto joinRoomRequestDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                if (userId == "")
                    return new ReturnModel(401, "User not found", null);
                Task<Users> userT = userService.GetUserById(userId);
                Task<Rooms> roomT = roomService.GetRoomByRoomKey(joinRoomRequestDto.RoomKey);
                await Task.WhenAll(userT, roomT);

                Users user = await userT;
                Rooms room = await roomT;

                if(user.CurrentActivity != null)
                    return new ReturnModel((int)HttpStatusCode.MethodNotAllowed, user.CurrentActivity.desc, joinRoomRequestDto.RoomKey);
                if (room == null)
                    return new ReturnModel(404, "Không tìm thấy phòng với key: " + joinRoomRequestDto.RoomKey, joinRoomRequestDto);
                await roomService.SendRequestToTeacher(room, new SubUser(user.Id, user.DislayName, user.AvatarUrl), joinRoomRequestDto.Cmd);

                return new ReturnModel(200, "Yêu cầu tham gia phòng thành công", null);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPut("RemoveStudentFromRoom")]
        public async Task<ReturnModel> RemoveStudentFromRoom(RemoveFromRoomDto removeFromRoomDto)
        {
            try
            {
                var res = await roomService.RemoveStudentFromRoom(removeFromRoomDto.RoomId, removeFromRoomDto.StudentId);
                if (res)
                {
                    return new ReturnModel(200, string.Empty, removeFromRoomDto);
                }
                return new ReturnModel(500, "Internal server error", removeFromRoomDto);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }

}
