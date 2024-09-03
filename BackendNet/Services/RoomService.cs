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
        public async Task<IEnumerable<Rooms>> GetRoomByUserId(string userId)
        {
            try
            {
                return await roomRepository.GetManyByKey(nameof(Rooms.Owner) + "." + nameof(Rooms.Owner.user_id), userId);
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
        public async Task ResponseRequestToStudent(string roomId, string studentId, bool res, string cmd)
        {
            try
            {
                await eduNimoHubContext.Clients.Group(studentId).SendAsync(cmd, roomId, studentId, res);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveStudentFromRoom(string roomId, string studentId)
        {
            try
            {
                return await roomRepository.RemoveStudentFromRoom(roomId, studentId);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
