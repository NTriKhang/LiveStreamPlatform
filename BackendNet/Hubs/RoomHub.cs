using Microsoft.AspNetCore.SignalR;

namespace BackendNet.Hubs
{
    public class RoomHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            string userId = Context.GetHttpContext().Request.Query["userId"].ToString() ?? "";
            string roomId = Context.GetHttpContext().Request.Query["roomId"].ToString() ?? "";
            Console.WriteLine("roomId : " + roomId);
            if (userId != "")
                Groups.AddToGroupAsync(Context.ConnectionId, userId);
            return base.OnConnectedAsync();
        }
    }
}
