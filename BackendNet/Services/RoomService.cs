using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Net;

namespace BackendNet.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository roomRepository;
        private readonly IUserService userService;
        private readonly IHubContext<EduNimoHub> eduNimoHubContext;
        public RoomService(
            IRoomRepository roomRepository
            , IUserService userService
            , IHubContext<EduNimoHub> eduNimoHubContext
        )
        {
            this.roomRepository = roomRepository;
            this.userService = userService;
            this.eduNimoHubContext = eduNimoHubContext;
        }
        public async Task<ReturnModel> AddRoom(Rooms room)
        {
            try
            {
                var returmModel = new ReturnModel();
                var isRemain = await IsUserRemainRoom(room.Owner.user_id);
                if(isRemain)
                {
                    returmModel.code = (int)HttpStatusCode.MethodNotAllowed;
                    returmModel.message = "Tồn tại phòng ở trạng thái chưa kết thúc";
                    returmModel.entity = room;
                    return returmModel;
                }
                var isStreamKeyInUse = await userService.IsStreamKeyInUse(room.Owner.user_id);
                if(isStreamKeyInUse)
                {
                    returmModel.code = (int)HttpStatusCode.MethodNotAllowed;
                    returmModel.message = "Stream key của người dùng đang được sử dụng";
                    returmModel.entity = room;
                    return returmModel;
                }

                var res = await roomRepository.Add(room);
                returmModel.entity = res;
                returmModel.code = 200;
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
                var res = await roomRepository.RemoveByKey(nameof(Rooms._id), roomId);
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
                return await roomRepository.GetByKey(nameof(Rooms._id), roomId);
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
                return await roomRepository.GetByKey(nameof(Rooms.RoomKey), roomKey);
            }
            catch (Exception)
            {

                throw;
            }
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

        public async Task<UpdateResult> AddStudentToRoom(string roomId, SubUser student)
        {
            try
            {
                var updateDef = Builders<Rooms>.Update.Push(x => x.Attendees, student);
                return await roomRepository.UpdateByKey(nameof(Rooms._id), roomId, null, updateDef);
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
                await eduNimoHubContext.Clients.Group(rooms.Owner.user_id).SendAsync(cmd, subUser, rooms._id);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
