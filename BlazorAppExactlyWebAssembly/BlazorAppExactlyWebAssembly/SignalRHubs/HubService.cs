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


//private readonly IHubContext<SignalRHubForBlazor> _blazorHubContext;// был private readonly IHubContext<SignalRHubForBlazor> _blazorHubContext;
// проблема в этом

//public async Task OnStopSendAudioChunk()
//{
//    await _hubContext.Clients.All.OnRemoveStream();
//}

//public async Task OnStreamStarted()
//{
//    await _blazorHubContext.Clients.All.SendAsync("OnStreamStarted");
//}

//public async Task OnStreamStopped()
//{
//    await _blazorHubContext.Clients.All.SendAsync("OnStreamStopped");
//}

//public async Task OnRemoveStream()
//{
//    await _blazorHubContext.Clients.All.SendAsync("OnRemoveStream");
//}

//public async Task OnAudioChunk(byte[] chunk)
//{
//    await _blazorHubContext.Clients.All.SendAsync("OnAudioChunk", chunk);
//}

// не знаю нужны ли будут эти методы
//public async Task NotifyAudioClientsStart()
//{
//    await _hubContext.Clients.All.OnStreamStarted();
//}
//public async Task NotifyAudioClientsStop()
//{
//    await _hubContext.Clients.All.OnStreamStopped();
//}
////

//public async Task SendAudioChunkToAll(byte[] chunk)
//{
//    await _hubContext.Clients.All.OnAudioChunk(chunk);
//}


