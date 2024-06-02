using BackendNet.Repositories.IRepositories;
using BackendNet.Services;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BackendNet.Hubs
{
    public class StreamHub : Hub<IStreamHub>
    {
        private readonly IUserService _userService;
        public StreamHub(IUserService userService)
        {
            _userService = userService;
        }
        [Authorize]
        public override Task OnConnectedAsync()
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId != null)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine("userId: " + userId + " connectionId: " + Context.ConnectionId);
                AddTokenToGroupsAsync(userId);
            }
            return base.OnConnectedAsync();
        }
        private Task AddTokenToGroupsAsync(string userId)
        {
            var user = _userService.GetUserById(userId).GetAwaiter().GetResult();
            var streamToken = user?.StreamInfo?.Stream_token;
            if (streamToken != null)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, streamToken);
                //Console.WriteLine("Streamkey: " + streamToken + " connectionId: " + Context.ConnectionId);

            }
            return Task.CompletedTask;
        }
        [Authorize]
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId != null)
            {
                Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                _ = RemoveTokenFromGroupsAsync(userId);
            }
            return base.OnDisconnectedAsync(exception);
        }
        private async Task RemoveTokenFromGroupsAsync(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

            var user = await _userService.GetUserById(userId);
            var streamToken = user?.StreamInfo?.Stream_token;
            if (streamToken != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, streamToken);
            }
        }
        //public async Task OnStopStreaming(string userId)
        //{
        //    string message = "stop streaming";
        //    await Clients.Group(userId).OnStopStreaming(message);
        //}
    }
}
