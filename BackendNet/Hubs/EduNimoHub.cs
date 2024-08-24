using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BackendNet.Hubs
{
    public class EduNimoHub : Hub
    {
        [Authorize]
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

    }
}
