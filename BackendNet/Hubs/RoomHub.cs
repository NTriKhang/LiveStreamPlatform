using BackendNet.Models;
using BackendNet.Services;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BackendNet.Dtos.HubDto;
using BackendNet.Dtos.HubDto.Room;
using AutoMapper;

namespace BackendNet.Hubs
{
    public class RoomHub : Hub
    {
        private readonly IChatliveService chatLiveService;
        private readonly IMapper mapper;
        public RoomHub(IChatliveService chatLiveService
            , IMapper mapper
            )
        {
            this.chatLiveService = chatLiveService;
            this.mapper = mapper;
        }
        public override Task OnConnectedAsync()
        {
            string roomId = Context.GetHttpContext().Request.Query["roomId"].ToString() ?? "";
            if (roomId != "")
                Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        [HubMethodName("SendChat")]
        [Authorize]
        public async Task sendChat(ChatDto chatLive)
        {
            try
            {
                var chat = new ChatLive();
                mapper.Map(chatLive, chat);

                chat.createdAt = DateTime.Now;
                await chatLiveService.AddChat(chat);
                await Clients.Group(chat.room_id).SendAsync(chatLive.Cmd, chat);
            }
            catch (Exception)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("onChatError");
            }
        }
    }
}
