using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace BackendNet.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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

        public async Task<Users> AuthUser(string username, string password)
        {
            return await _userRepository.AuthAsync(username, password);   
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
    }
}
