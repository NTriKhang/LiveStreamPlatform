using Microsoft.AspNetCore.SignalR;

namespace BackendNet.Hubs
{
    public class RoomHub : Hub
    {
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
    }
}
