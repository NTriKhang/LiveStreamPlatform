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

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IRoomService roomService;
        private readonly IVideoService videoService;
        public RoomController(IRoomService roomService
            , IMapper mapper
            , IVideoService videoService)
        {
            this.mapper = mapper;
            this.roomService = roomService;
            this.videoService = videoService;
        }
        [HttpGet("{roomId}")]
        public async Task<ActionResult<Rooms>> GetRoom(string roomId)
        {
            try
            {
                var rooms = await roomService.GetRoomById(roomId);

                if (rooms == null)
                    return NoContent();

                //_ = videoService.UpdateVideoView(rooms.Video.Id!);
                if (rooms.Status == RoomStatus.Closed.ToString())
                    return StatusCode(StatusCodes.Status406NotAcceptable);
                else if (rooms.Status == RoomStatus.Expired.ToString())
                    return StatusCode(StatusCodes.Status404NotFound);
                return Ok(rooms);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public async Task<ActionResult<Rooms>> AddRoom(RoomCreateDto roomCreate)
        {
            try
            {
                Rooms room = new Rooms();
                mapper.Map(roomCreate,room);
                var res = await roomService.AddRoom(room);
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
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
