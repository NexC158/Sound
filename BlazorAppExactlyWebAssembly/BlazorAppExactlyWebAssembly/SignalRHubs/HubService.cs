using Microsoft.AspNetCore.SignalR;

namespace BlazorAppExactlyWebAssembly.SignalRHubs
{
    public class HubService
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public HubService(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task TransferStartInvoke(string method)
        {
            await _hubContext.Clients.All.SendAsync(method);
        }
    }
}
