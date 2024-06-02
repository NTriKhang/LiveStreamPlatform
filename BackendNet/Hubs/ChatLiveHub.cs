using BackendNet.HubDto;
using BackendNet.Models;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BackendNet.Hubs
{
    public class ChatLiveHub : Hub
    {
        private readonly IChatliveService chatLiveService;
        public ChatLiveHub(IChatliveService chatLiveService)
        {

            this.chatLiveService = chatLiveService;

        }
        public override async Task OnConnectedAsync()
        {
            try
            {
                Console.WriteLine("Connect to chatlive");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in OnConnectedAsync: {ex.Message}");
                throw;
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                Console.WriteLine("Disconnect to chatlive");
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in OnDisconnectedAsync: {ex.Message}");
                throw;
            }
        }
        [HubMethodName("ConnectToRoom")]
        [Authorize]
        public async Task roomConnect(string roomId)
        {
            try
            {                
                Console.WriteLine($"in room connect {roomId}");
                string message = $"{Context.User?.FindFirstValue(ClaimTypes.Name)} has joined the room.";
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                _ = Clients.Group(roomId).SendAsync("onRoomConnected", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RoomConnect: {ex.Message}");
                throw; // This rethrows the caught exception to be handled by SignalR
            }
        }
        [HubMethodName("SendChat")]
        [Authorize]
        public async Task sendChat(ChatLive chatLive)
        {
            try
            {
                chatLive.createdAt = DateTime.Now;

                var chat = await chatLiveService.AddChat(chatLive);
                await Clients.Group(chatLive.room_id).SendAsync("onChatLive", chat);
            }
            catch (Exception)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("onChatError");
            }
        }
    }
}
