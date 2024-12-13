using BackendNet.Models;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatLiveController : ControllerBase
    {
        private readonly IChatliveService _chatliveService;
        private readonly IRoomService roomService;

        public ChatLiveController(IChatliveService chatliveService,
            IRoomService roomService)
        {
            _chatliveService = chatliveService;
            this.roomService = roomService;

        }

        [HttpGet("{roomId}")]
        public async Task<PaginationModel<ChatLive>> getChatLives(string roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Chat)
        {
            try
            {
                return await _chatliveService.GetChatsPagination(roomId, page, pageSize);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
