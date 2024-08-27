using BackendNet.Models.Submodel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BackendNet.Hubs
{
    public interface IEduNimoHub
    {
        Task SendNotifyRequestToRoomOnwer(SubUser student, string roomOwnerId , string cmd);
        Task SendNotifyNewUserToRoom(SubUser student, string roomId, string cmd);
    }
    public class EduNimoHub : Hub
    {
        const string ErrorReturn = "ReceiveError";

        public override Task OnConnectedAsync()
        {
            string userId = Context.GetHttpContext().Request.Query["userId"].ToString() ?? "";
            Console.WriteLine("userId : " + userId);
            if(userId != "")
                Groups.AddToGroupAsync(Context.ConnectionId, userId);
            return base.OnConnectedAsync();
        }

        [Authorize]
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.GetHttpContext().User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if(userId != "")
                Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendError(string error)
        {
            await Clients.Client(Context.ConnectionId).SendAsync(error);
        }
    }
}
