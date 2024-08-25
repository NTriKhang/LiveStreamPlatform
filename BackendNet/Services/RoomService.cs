﻿using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository roomRepository;
        public RoomService(IRoomRepository roomRepository)
        {
            this.roomRepository = roomRepository;
        }
        public async Task<Rooms> AddRoom(Rooms room)
        {
            try
            {
                return await roomRepository.Add(room);
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

        public async Task<UpdateResult> UpdateRoomStatus(int status, string roomKey)
        {
            try
            {
                var updateDefine = Builders<Rooms>.Update.Set(x => x.Status, status);
                return await roomRepository.UpdateByKey("RoomKey", roomKey, null, updateDefine);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
