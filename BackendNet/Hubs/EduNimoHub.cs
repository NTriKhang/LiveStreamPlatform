using BackendNet.Models.Submodel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BackendNet.Hubs
{
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

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
