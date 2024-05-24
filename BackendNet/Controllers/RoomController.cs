using BackendNet.Dtos;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
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
        private readonly IRoomService roomService;
        private readonly IVideoService videoService;
        public RoomController(IRoomService roomService, IVideoService videoService)
        {
            this.roomService = roomService;
            this.videoService = videoService;
        }
        [HttpGet("{roomKey}")]
        public async Task<ActionResult<Rooms>> GetRoom(string roomKey)
        {
            try
            {
                var rooms = await roomService.GetRoomByRoomKey(roomKey);
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
        [HttpPut]
        public async Task<ActionResult<UpdateResult>> UpdateRoom(UpdateRoomStatusDto updateRoomStatusDto)
        {
            try
            {
                var res = await roomService.UpdateRoomStatus(updateRoomStatusDto.status, updateRoomStatusDto.roomKey);
                return Ok(res.IsAcknowledged);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }            
        }
    }

}
