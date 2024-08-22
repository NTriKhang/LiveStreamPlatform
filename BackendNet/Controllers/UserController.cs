using AutoMapper;
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
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly IHubContext<StreamHub> hubContext;
        public UserController(IUserService userService
            , IMapper mapper
            , IHubContext<StreamHub> hubContext)
        {
            this.userService = userService;
            this.hubContext = hubContext;
            this.mapper = mapper;
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
        public async Task<ActionResult> login(UserLoginDto user)
        {
            try
            {
                var userAuth = await userService.AuthUser(user.UserName, user.Password);
                if (userAuth == null)
                {
                    return NotFound(user);
                }
                if(userAuth.code == 300)
                {
                    return StatusCode(StatusCodes.Status303SeeOther, user);
                }
                var expired_time = DateTime.Now.AddDays(6); 

                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.HttpOnly = true;
                cookieOptions.Expires = expired_time;
                // string url = Request.GetDisplayUrl();
                // if (url.Contains(".hightfive.click"))
                // {
                //     cookieOptions.Domain = ".hightfive.click";
                //     cookieOptions.Secure = true;
                // }
                var token = GenerateJWTToken((userAuth.entity as Users)!);
                Response.Cookies.Append("AuthToken", token, cookieOptions);
                    
                return Ok(userAuth.entity);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [NonAction]
        public string GenerateJWTToken(Users user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, user.Role),
            };
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                       Encoding.UTF8.
                            GetBytes("this is my top jwt secret key for authentication and i append it to have enough lenght")
                        ),
                    SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
        [HttpPost("logout")]
        public async Task<ActionResult<Users>> Logout()
        {
            try
            {
                Response.Cookies.Append("AuthToken", "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });
                return NoContent();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPut("updateProfile")]
        public async Task<ActionResult> UpdateProfile(UserProfileDto userProfileDto)
        {
            try
            {
                var user = await userService.GetUserById(userProfileDto.Id);
                if (user == null)
                    return BadRequest();
                
                mapper.Map<UserProfileDto,Users>(userProfileDto, user);
                var res = await userService.UpdateUser(user);
                if (res)
                    return NoContent();
                return BadRequest(userProfileDto);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
