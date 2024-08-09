using BackendNet.Models;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService followService;
        public FollowController(IFollowService followService)
        {
            this.followService = followService;
        }
        // GET: api/<FollowController>
        [HttpGet("GetFollower/{followed_id}")]
        public async Task<IEnumerable<Follow>> GetFollower(string followed_id, [FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Follow)
        {
            return await followService.GetFollower(followed_id, page);
            }
        // GET: api/<FollowController>
        [HttpGet("GetFollowing/{follower_id}")]
        public async Task<IEnumerable<Follow>> GetFollowing(string follower_id, [FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Follow)
        {
            return await followService.GetFollowing(follower_id, page);
        }

        //// GET api/<FollowController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<FollowController>
        [HttpPost("PostFollow")]
        public async Task<ActionResult> Post([FromBody] Follow follow)
        {
            var res = await followService.PostFollow(follow);
            return CreatedAtAction("GetFollower", new { followed_id = follow.Followed.user_id}, res);
        }

        //// PUT api/<FollowController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<FollowController>/5
        [HttpDelete("RemoveFollow/{followId}")]
        public async Task<ActionResult> Delete(string followId)
        {
            bool res = await followService.RemoveFollow(followId);
            if(res == false)
                return BadRequest(followId);
            return NoContent();
        }
    }
}
