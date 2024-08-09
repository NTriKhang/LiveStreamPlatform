using BackendNet.Dtos;
using BackendNet.Models;
using BackendNet.Models.Submodel;
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
        private readonly IHttpContextAccessor _contextAccessor;
        public UserService(
            IUserRepository userRepository
            //, IConnectionMultiplexer redisConnect
            , IHttpContextAccessor contextAccessor

        )
        {
            _userRepository = userRepository;
            //_redisConnect = redisConnect;
            _contextAccessor = contextAccessor;

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

            if (user.Role == RoleKey.Teacher.ToString())
                user.StreamInfo = new Models.Submodel.StreamInfo();
            
            
            return await _userRepository.Add(user);
        }

        public async Task<ReturnModel> AuthUser(string username, string password)
        {
            var user = await _userRepository.AuthAsync(username, password);
            if (user == null)
                return null!;
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
        public async Task<UpdateResult> UpdateStreamStatusAsync(string user_id, string status)
        {
            return await _userRepository.UpdateStreamTokenAsync(user_id, status);
        }
        public async Task<IEnumerable<Users>> GetUsersAsync()
        {
            return await _userRepository.GetAll();
        }

        public async Task<Users> GetUserById(string id)
        {
            return await _userRepository.GetByKey(nameof(Users.Id) ,id);
        }

        public async Task<bool> IsTokenExist(string streamKey)
        {
            if ((await _userRepository.GetByKey("StreamInfo.Stream_token", streamKey)) != null)
            {
                return true;
            }
            return false;
        }

        public async Task<Users> GetUserByStreamKey(string streamKey)
        {
            return await _userRepository.GetByKey("StreamInfo.Stream_token", streamKey);
        }

        public async Task<SubUser> GetSubUser(string id)
        {
            var projection = Builders<Users>.Projection
                                        .Include(x => x.Id)
                                        .Include(x => x.DislayName)
                                        .Include(x => x.AvatarUrl);
            var user = await _userRepository.GetByKey(nameof(Users.Id), id, projection);
            return new SubUser(user.Id, user.DislayName, user.AvatarUrl);
        }
    }
}
