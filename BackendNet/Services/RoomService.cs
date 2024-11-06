using BackendNet.Hubs;
using BackendNet.Dtos.HubDto.Room;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Net;
using System.Security.Claims;

namespace BackendNet.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository roomRepository;
        private readonly IUserService userService;
        private readonly IHubContext<EduNimoHub> eduNimoHubContext;
        private readonly IHubContext<RoomHub> roomHubContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        public RoomService(
            IRoomRepository roomRepository
            , IUserService userService
            , IHubContext<EduNimoHub> eduNimoHubContext
            , IHubContext<RoomHub> roomHubContext
            , IHttpContextAccessor httpContextAccessor
        )
        {
            this.roomRepository = roomRepository;
            this.userService = userService;
            this.eduNimoHubContext = eduNimoHubContext;
            this.roomHubContext = roomHubContext;
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<ReturnModel> AddRoom(Rooms room)
        {
            try
            {
                var returmModel = new ReturnModel();
                var isRemain = IsUserRemainRoom(room.Owner.user_id);
                var isStreamKeyInUse = userService.IsStreamKeyValid(room.Owner.user_id);

                await Task.WhenAll(isRemain, isStreamKeyInUse);
                if(await isRemain)
                {
                    returmModel.code = (int)HttpStatusCode.MethodNotAllowed;
                    returmModel.message = "Tồn tại phòng ở trạng thái chưa kết thúc";
                    returmModel.entity = room;
                    return returmModel;
                }
                if(await isStreamKeyInUse == false)
                {
                    returmModel.code = (int)HttpStatusCode.MethodNotAllowed;
                    returmModel.message = "Stream key không tồn tại hoặc đang được sử dụng";
                    returmModel.entity = room;
                    return returmModel;
                }

                var res = await roomRepository.AddRoom(room);
                returmModel.entity = res;
                returmModel.code = 200;

                //_ = userService.UpdateStreamStatusAsync(room.Owner.user_id, StreamStatus.Streaming.ToString());

                return returmModel;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<bool> IsUserRemainRoom(string userId)
        {
            try
            {
                var filterUserId = Builders<Rooms>.Filter.Eq(x => x.Owner.user_id, userId);


                var filterStatus = Builders<Rooms>.Filter.Or(
                    Builders<Rooms>.Filter.Eq(x => x.Status, (int)RoomStatus.Opening),
                    Builders<Rooms>.Filter.Eq(x => x.Status, (int)RoomStatus.Closed)
                );
                var filter = Builders<Rooms>.Filter.And(filterUserId, filterStatus);
                return await roomRepository.IsExist(filter);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> DeleteRoom(string roomId)
        {
            try
            {
                var filter = Builders<Rooms>.Filter.Eq(x => x._id, roomId);
                var room = await roomRepository.GetByFilter(filter);
                if (room == null)
                    return false;
                var res = await roomRepository.RemoveRoom(room);
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Rooms> GetRoomById(string roomId)
        {
            try
            {
                var filDef = Builders<Rooms>.Filter.Eq(x => x._id, roomId);
                return await roomRepository.GetByFilter(filDef);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<Rooms>> GetRoomByUserId(string userId)
        {
            try
            {
                var filter = Builders<Rooms>.Filter.Eq(x => x.Owner.user_id, userId);
                var sort = Builders<Rooms>.Sort.Descending(x => x.CDate);
                return await roomRepository.GetAll(filter, sort);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<Rooms> GetRoomByRoomKey(string roomKey)
        {
            try
            {
                var filDef = Builders<Rooms>.Filter.Eq(x => x.RoomKey, roomKey);
                return await roomRepository.GetByFilter(filDef);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<Rooms> GetActiveRoomByStreamKey(string streamKey)
        {
            var user = await userService.GetUserByStreamKey(streamKey);

            if (user.CurrentActivity == null || user.CurrentActivity.code != "Room")
                return null;

            var filter = Builders<Rooms>.Filter.Eq(x => x._id, user.CurrentActivity.value);
            return await roomRepository.GetByFilter(filter);
        }
        public async Task<ReplaceOneResult> UpdateRoom(Rooms room)
        {
            try
            {
                var filter = Builders<Rooms>.Filter.Eq(x => x._id, room._id);
                return await roomRepository.ReplaceAsync(filter, room);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddStudentToRoom(string roomId, SubUser student)
        {
            try
            {
                return await roomRepository.AddStudentToRoom(roomId, student);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task SendRequestToTeacher(Rooms rooms, SubUser subUser, string cmd)
        {
            try
            {
                await eduNimoHubContext.Clients.Group(rooms.Owner.user_id).SendAsync(cmd
                    , new ResponseRoomJoinRqDto(subUser.user_id
                    , subUser.user_name
                    , subUser.user_avatar
                    , rooms._id));
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task ResponseRequestToStudent(ResponseRoomRequestDto response)
        {
            try
            {
                if (response.Res)
                {
                    var room = await roomRepository.GetByFilter(Builders<Rooms>.Filter.Eq(x => x._id, response.RoomId));
                    if (room != null)
                        response.RoomType = room.RoomType;
                }

                await eduNimoHubContext.Clients.Group(response.StudentId).SendAsync(response.Cmd, response);
                if(response.Res)
                    await roomHubContext.Clients.Group(response.RoomId).SendAsync(response.Cmd, response);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveFromRoom(RemoveFromRoomDto removeFromRoomDto)
        {
            try
            {
                bool res = false;
                var filter = Builders<Rooms>.Filter.Eq(x => x.RoomKey, removeFromRoomDto.RoomId);
                var room = await roomRepository.GetByFilter(filter);

                if (room == null)
                    return false;

                if(room.Owner.user_id == removeFromRoomDto.UserId)
                    res = await roomRepository.RemoveRoom(room);
                else
                    res = await roomRepository.RemoveFromRoom(room._id, removeFromRoomDto.UserId);
                   
                if (res)
                    await roomHubContext.Clients.Group(removeFromRoomDto.RoomId).SendAsync(removeFromRoomDto.Cmd, removeFromRoomDto);

                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsRoomHasUserId(string roomKey, string userId)
        {
            var filter = Builders<Rooms>.Filter.And(
                Builders<Rooms>.Filter.Eq(x => x.RoomKey, roomKey),
                Builders<Rooms>.Filter.Or(
                    Builders<Rooms>.Filter.ElemMatch(x => x.Attendees, x => x.user_id == userId),
                    Builders<Rooms>.Filter.Eq(x => x.Owner.user_id, userId)
                )
            );
            return await roomRepository.IsExist(filter);
        }
    }
}
