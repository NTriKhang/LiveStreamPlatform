using AutoMapper;
using BackendNet.Dtos;
using BackendNet.Dtos.Room;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
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
        public RoomController(IRoomService roomService
            , IMapper mapper
            , IVideoService videoService
            , IUserService userService
            )
        {
            this.mapper = mapper;
            this.roomService = roomService;
            this.videoService = videoService;
            this.userService = userService; 
        }
        /// <summary>
        /// chưa xài
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [HttpGet("{roomId}")]
        public async Task<ActionResult<Rooms>> GetRoom(string roomId)
        {
            try
            {
                var rooms = await roomService.GetRoomById(roomId);

                if (rooms == null)
                    return NoContent();

                //_ = videoService.UpdateVideoView(rooms.Video.Id!);
                if (rooms.Status == (int)RoomStatus.Closed)
                    return StatusCode(StatusCodes.Status406NotAcceptable);
                else if (rooms.Status == (int)RoomStatus.Expired)
                    return StatusCode(StatusCodes.Status404NotFound);
                return Ok(rooms);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Chưa xài
        /// </summary>
        /// <param name="roomCreate"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Rooms>> AddRoom(RoomCreateDto roomCreate)
        {
            try
            {
                Rooms room = new Rooms();
                mapper.Map(roomCreate,room);

                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                if (userId == string.Empty)
                    return StatusCode(StatusCodes.Status404NotFound, "Không tìm thấy user");

                var user = await userService.GetUserById(userId);

                if (user.StreamInfo != null && user.StreamInfo.Stream_token != null)
                {
                    room.StreamKey = user.StreamInfo.Stream_token;
                    room.CDate = DateTime.Now;
                    room.Attendees = new List<Models.Submodel.SubUser>();
                    room.Status = roomCreate.Status;
                    room.Owner = new Models.Submodel.SubUser(user.Id, user.DislayName, user.AvatarUrl);
                    var res = await roomService.AddRoom(room);
                    return Ok(res);
                }
                else
                {
                    return StatusCode(StatusCodes.Status307TemporaryRedirect, "Cần cập nhật lại stream key");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        /// <summary>
        /// Chưa xài
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
        //[HttpPut]
        //public async Task<ActionResult<UpdateResult>> UpdateRoom(UpdateRoomStatusDto updateRoomStatusDto)
        //{
        //    try
        //    {
        //        var res = await roomService.UpdateRoomStatus(updateRoomStatusDto.status, updateRoomStatusDto.roomKey);
        //        return Ok(res.IsAcknowledged);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        //    }            
        //}
    }

}
