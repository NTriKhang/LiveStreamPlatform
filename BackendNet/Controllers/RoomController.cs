using AgoraIO.Media;
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
using Org.BouncyCastle.Asn1.Cmp;
using System;
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
        private readonly IUserService userService;
        private readonly IStatusService statusService;
        public RoomController(IRoomService roomService
            , IMapper mapper
            , IUserService userService
            , IStatusService statusService
            )
        {
            this.mapper = mapper;
            this.roomService = roomService;
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
                if (roomId == string.Empty)
                    return new ReturnModel(400, "", "");

                var rooms = await roomService.GetRoomById(roomId);

                if (rooms == null)
                    return new ReturnModel(400, "Không tìm thấy phòng này", roomId);

                //_ = videoService.UpdateVideoView(rooms.Video.Id!);
                if (rooms.Status == (int)RoomStatus.Closed)
                    return new ReturnModel(400, "Phòng này hiện đang đóng", roomId);
                else if (rooms.Status == (int)RoomStatus.Expired)
                    return new ReturnModel(400, "Phòng này đã hết hạn", roomId);

                string streamUrl = "rtmp://192.168.18.219/live";

                var user = await userService.GetUserById(rooms.Owner.user_id);
                if (user == null)
                    return new ReturnModel(404, "User not found", roomId);

                string streamKey = user.StreamInfo?.Stream_token ?? "";

                object returnObj = new {streamUrl = streamUrl, streamKey = streamKey, room = rooms};

                return new ReturnModel(200, string.Empty, returnObj);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("GetRoomByKey/{roomKey}")]
        public async Task<ReturnModel> GetRoomByKey(string roomKey)
        {
            try
            {
                if (roomKey == string.Empty)
                    return new ReturnModel(400, "", "");

                var rooms = await roomService.GetRoomByRoomKey(roomKey);

                if (rooms == null)
                    return new ReturnModel(400, "Không tìm thấy phòng này", roomKey);

                //_ = videoService.UpdateVideoView(rooms.Video.Id!);
                if (rooms.Status == (int)RoomStatus.Closed)
                    return new ReturnModel(400, "Phòng này hiện đang đóng", roomKey);
                else if (rooms.Status == (int)RoomStatus.Expired)
                    return new ReturnModel(400, "Phòng này đã hết hạn", roomKey);

                string streamUrl = "rtmp://192.168.18.219/live";

                var user = await userService.GetUserById(rooms.Owner.user_id);
                if (user == null)
                    return new ReturnModel(404, "User not found", roomKey);

                string streamKey = user.StreamInfo?.Stream_token ?? "";

                object returnObj = new { streamUrl = streamUrl, streamKey = streamKey, room = rooms };

                return new ReturnModel(200, string.Empty, returnObj);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Đăng nhập trước khi sử dụng
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [HttpGet("GetUserRoom")]
        [Authorize]
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
        /// Đăng nhập trước khi sử dụng
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
                    room.RoomType = roomCreate.RoomType;
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
                    await roomService.ResponseRequestToStudent(resRoomRequest);
                    return new ReturnModel(200, string.Empty, null);
                }

                var subUser = await userService.GetSubUser(resRoomRequest.StudentId);
                var res = await roomService.AddStudentToRoom(resRoomRequest.RoomId, subUser);
                if (res)
                {
                    Task response = roomService.ResponseRequestToStudent(resRoomRequest);
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
        /// Đăng nhập trước khi sử dụng
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
        [HttpPut("RemoveFromRoom")]
        [Authorize]
        public async Task<ReturnModel> RemoveFromRoom(RemoveFromRoomDto removeFromRoomDto)
        {
            try
            {
                if (removeFromRoomDto.UserId == null || removeFromRoomDto.UserId == "")
                    removeFromRoomDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var res = await roomService.RemoveFromRoom(removeFromRoomDto);
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
        [HttpGet("GenerateMeetingToken")]
        [Authorize]
        public async Task<ReturnModel> GenerateRtcToken(string channelName)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var room = await roomService.GetRoomByRoomKey(channelName);

            if (room == null)
                return new ReturnModel(404, "Phòng học không tồn tại", null);

            string userName = "";

            if (room.Attendees.Where(x => x.user_id == userId).Any())
                userName = room.Attendees.Where(x => x.user_id == userId).Select(x => x.user_name).FirstOrDefault();

            if(room.Owner.user_id == userId)
                userName = room.Owner.user_name;

            if(userName == "")
                return new ReturnModel(400, "Không tìm thấy phòng hoặc người dùng không có trong phòng", null);
            
            string _appId = "2a1f82d264aa44cd915e30f6bfd0505a";
            //string _appCertificate = "4f3398988a4f41d0b23b641d30a5bb92";
            string userRole = "";
            if (room.Owner.user_id == userId)
                userRole = "host";
            else
                userRole = "audience";

            return new ReturnModel(200, "", new
            {
                role = userRole,
                appId = _appId,
                userName = userName,
                screenSharing = userRole == "host" ? true : false,
                showPopUpBeforeRemoteMute = userRole == "host" ? true : false,
                disableRtm = userRole == "host" ? false : true, 
            });
        }
    }

}
