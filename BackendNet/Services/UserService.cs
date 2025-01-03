﻿using BackendNet.Dtos;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BackendNet.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        //private readonly IConnectionMultiplexer _redisConnect;
        private readonly ISysRelationshipRepo _sysRelationshipRepo;
        public UserService(
            IUserRepository userRepository
        //, IConnectionMultiplexer redisConnect
            , ISysRelationshipRepo sysRelationshipRepo
        )
        {
            _userRepository = userRepository;
            _sysRelationshipRepo = sysRelationshipRepo;
            //_redisConnect = redisConnect;

        }
        public async Task<Users> AddUserAsync(Users user)
        {
            var filterEmail = Builders<Users>.Filter.Eq(u => u.Email, user.Email);
            var filterUserName = Builders<Users>.Filter.Eq(u => u.UserName, user.UserName);

            var task1 = Task.Run(() => _userRepository.IsExist(filterEmail));
            var task2 = Task.Run(() => _userRepository.IsExist(filterUserName));

            await Task.WhenAll(task1, task2);

            if (task1.Result)
                return new Users() { Email = "409" };
            else if (task2.Result)
                return new Users() { UserName = "409" };
            
            user.Password = CryptPassword(user.Password);
            return await _userRepository.Add(user);
        }
        public async Task<ReturnModel> AuthUser(string username, string password)
        {
            var filterDef = Builders<Users>.Filter.Eq(x => x.UserName, username);
            var user = await _userRepository.GetByFilter(filterDef);

            if (user == null)
                return null!;

            if (user.IsScript && !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new ReturnModel(400, "Sai mật khẩu", null);
            }

            if (user.IsScript == false)
            {
                var newPassword = CryptPassword(user.Password);

                user.IsScript = true;
                user.Password = newPassword;

                var filter = Builders<Users>.Filter.Eq(x => x.Id, user.Id);
                _ = _userRepository.ReplaceAsync(filter, user);
            }
            return new ReturnModel(200, string.Empty, user);

            //var userAgent = _contextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;
            //string device = string.Empty;
            //if(userAgent != string.Empty)
            //{
            //    if (userAgent.Contains("Windows NT") || userAgent.Contains("Macintosh") || userAgent.Contains("Mac OS X"))
            //        device = "web";
            //    else if (userAgent.Contains("Mobi") || userAgent.Contains("Android") || userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            //        device = "mobile";
            //    else
            //        device = "tool";
            //}
            //if(device == "tool")
            //    return new ReturnModel(200, string.Empty, user);

            //var db = _redisConnect.GetDatabase();
            try
            {
            //    var redisVal = await db.SetMembersAsync(user.Id);
            //    var res = redisVal.Where(x => x.ToString().Contains(device)).SingleOrDefault();
            //    if (res.HasValue)
            //    {
            //        return new ReturnModel(300, string.Empty, user);
            //    }

            //    var eleWhiteList = new JwtWhiteList();
            //    eleWhiteList.UserAgent = device;
            //    eleWhiteList.JwtId = Guid.NewGuid().ToString();
            //    var eleJson = JsonConvert.SerializeObject(eleWhiteList);

            //    await db.SetAddAsync(user.Id, eleJson);
               

                return new ReturnModel(200,string.Empty, user);
            }
            catch (Exception)
            {
                //await db.KeyDeleteAsync(user.Id);
                throw;
            }
        }
        private string CryptPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public async Task<PaginationModel<Users>> GetUsersAsync(int page, int pageSize, string? userName = null)
        {
            FilterDefinition<Users> filter = null;
            if (userName != null)
                filter = Builders<Users>.Filter.Regex(x => x.UserName, new MongoDB.Bson.BsonRegularExpression(userName, "i"));
            else
                filter = Builders<Users>.Filter.Empty;

            return await _userRepository.GetManyByFilter(page, pageSize, filter, Builders<Users>.Sort.Descending(x => x.Id));
        }
        public async Task<Users> GetUserById(string id)
        {
            var filterDef = Builders<Users>.Filter.Eq(x => x.Id, id);
            var user = await _userRepository.GetByFilter(filterDef);
            
            return user;
        }
        public async Task<Users> GetUserByStreamKey(string streamKey)
        {
            var filter = Builders<Users>.Filter.Eq(x => x.StreamInfo.Stream_token, streamKey);
            return await _userRepository.GetByFilter(filter);
        }
        public async Task<SubUser> GetSubUser(string id)
        {
            var filter = Builders<Users>.Filter.Eq(x => x.Id, id);

            var user = await _userRepository.GetByFilter(filter);
            return new SubUser(user.Id, user.DislayName, user.AvatarUrl);
        }
        public async Task<bool> IsStreamKeyValid(string userId)
        {
            var filter = Builders<Users>.Filter.And(
                Builders<Users>.Filter.Eq(x => x.Id, userId),
                Builders<Users>.Filter.Exists(x => x.StreamInfo.Stream_token),
                Builders<Users>.Filter.Eq(x => x.StreamInfo.Status, StreamStatus.Idle.ToString())
            );
            return await _userRepository.IsExist(filter);
        }
        public async Task<bool> IsStreamKeyExist(string streamKey)
        {
            var filter = Builders<Users>.Filter.Eq(x => x.StreamInfo.Stream_token, streamKey);
            if ((await _userRepository.GetByFilter(filter)) != null)
            {
                return true;
            }
            return false;
        }
        public async Task<UpdateResult> UpdateStreamStatusAsync(string user_id, string status)
        {
            var updDef = Builders<Users>.Update.Set(x => x.StreamInfo.Status, status);
            var filterDef = Builders<Users>.Filter.Eq(x => x.Id, user_id);
            return await _userRepository.UpdateByFilter(filterDef, updDef);
        }
        public async Task<bool> UpdateUser(Users user)
        {
            try
            {
                var filter = Builders<Users>.Filter.Eq(x => x.Id, user.Id);
                var res = await _userRepository.ReplaceAsync(filter, user);
                if (res.ModifiedCount == 0)
                    return false;

                _ = Task.Run(async () =>
                {
                    await _sysRelationshipRepo.UpdateUserMonitor(user);
                });

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<UpdateResult> UpdateStreamKey(string userId, Models.Submodel.StreamInfo streamInfo)
        {
            try
            {
                if (streamInfo == null)
                    streamInfo = new Models.Submodel.StreamInfo();

                streamInfo.Status = StreamStatus.Idle.ToString();
                streamInfo.Stream_token = Utility.GenerateStreamKey(10);
                streamInfo.Last_stream = null;

                var filterDef = Builders<Users>.Filter.Eq(x => x.Id, userId);
                var updateDef = Builders<Users>.Update.Set(x => x.StreamInfo, streamInfo);
                return await _userRepository.UpdateByFilter(filterDef, updateDef);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<UpdateResult> UpdateUserAcitivity(string userId, CurrentActivity currentActivity)
        {
            try
            {
                var filterDef = Builders<Users>.Filter.Eq(x => x.Id, userId);
                var udateDef = Builders<Users>.Update.Set(x => x.CurrentActivity, currentActivity);
                return await _userRepository.UpdateByFilter(filterDef, udateDef);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<UpdateResult> UpdateIncome(string userId, SubTrade subTrade)
        {
            var filterDef = Builders<Users>.Filter.Eq(x => x.Id, userId);
            var updateDef = Builders<Users>.Update.PushEach(x => x.Incomes, new[] { subTrade }, position: 0);

            return await _userRepository.UpdateByFilter(filterDef, updateDef);
        }
        public async Task<UpdateResult> UpdateOutcome(string userId, SubTrade subTrade)
        {
            var filterDef = Builders<Users>.Filter.Eq(x => x.Id, userId);
            var updateDef = Builders<Users>.Update.PushEach(x => x.Outcomes, new[] { subTrade }, position: 0);

            return await _userRepository.UpdateByFilter(filterDef, updateDef);
        }
    }
}
