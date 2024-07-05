using BackendNet.Dtos;
using BackendNet.Dtos.User;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Claims;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IHubContext<StreamHub> hubContext;
        public UserController(IUserService userService, IHubContext<StreamHub> hubContext)
        {
            this.userService = userService;
            this.hubContext = hubContext;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> getUser()
        {
            try
            {
                string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                return Ok(await userService.GetUserById(user_id));
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("{userId}")]
        public async Task<ActionResult> getUser(string userId)
        {
            try
            {
                Users user = await userService.GetUserById(userId);
                return Ok(new UserProfileDto(user.Id, user.UserName, user.Email, user.DislayName, user.Role, user.AvatarUrl));
            }
            catch (Exception)
            {

                throw;
            }
        }
        //[HttpGet("{token}")]
        //public async Task<Users> getUser(string token)
        //{
        //    try
        //    {
        //        return await userService.GetUserByStreamKey(token);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        [HttpPost]
        public async Task<ActionResult<Users>> signup(UserSignupDto user)
        {
            try
            {
                var newUser = await userService.AddUserAsync(
                    new Users
                    {
                        UserName = user.UserName,
                        Password = user.Password,
                        Email = user.Email,
                        DislayName = user.DislayName,
                        Role = user.Role,
                        AvatarUrl = "https://thumbs.dreamstime.com/b/default-avatar-profile-icon-vector-social-media-user-photo-183042379.jpg"
                    }
                );
                
                if (newUser.UserName == "409")
                    return Conflict("User name is already used");
                else if (newUser.Email == "409")
                    return Conflict("Email is already used");
                return CreatedAtAction(nameof(signup), newUser);
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        [HttpPost("auth")]
        public async Task<ActionResult<Users>> login(UserLoginDto user)
        {
            try
            {
                var userAuth = await userService.AuthUser(user.UserName, user.Password);
                if (userAuth == null)
                {
                    return NotFound(user);
                }
                var claims = new List<Claim>
                {
                    new Claim(type: ClaimTypes.NameIdentifier,value: userAuth.Id!),
                    new Claim(type: ClaimTypes.Name, value: userAuth.UserName), 
                    new Claim(type: ClaimTypes.UserData, value: userAuth.StreamInfo.Stream_token!)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                    }
                );

                return Ok(userAuth);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
