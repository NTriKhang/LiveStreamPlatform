using BackendNet.Models;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatLiveController : ControllerBase
    {
        private readonly IChatliveService _chatliveService;
        public ChatLiveController(IChatliveService chatliveService)
        {
            _chatliveService = chatliveService;
        }

        [HttpGet("{roomId}")]
        public async Task<IEnumerable<ChatLive>> getChatLives(string roomId,[FromQuery] int page = 1)
        {
            try
            {
                return await _chatliveService.GetChatsPagination(roomId, page);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
