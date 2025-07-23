using Microsoft.AspNetCore.SignalR;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public class HubService : Hub, IAudioStreamReceiver
{
    private readonly IHubContext<SignalRHub, IAudioStreamReceiver> _hubContext;

    public HubService(IHubContext<SignalRHub, IAudioStreamReceiver> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task TransferInvokeStart() // string method
    {
        await _hubContext.Clients.All.OnCustomCommandStart();
    }

    public async Task TransferInvokeStop() // string method
    {
        await _hubContext.Clients.All.OnCustomCommandStop();
    }
    public Task OnCustomCommandStart()
    {
        throw new NotImplementedException();
    }

    public Task OnCustomCommandStop()
    {
        throw new NotImplementedException();
    }
}
