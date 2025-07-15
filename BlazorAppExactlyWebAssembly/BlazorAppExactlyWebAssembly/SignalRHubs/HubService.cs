using Microsoft.AspNetCore.SignalR;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared2;

public class HubService : IAudioStreamReceiver// это мой паблишер
{
    private readonly IHubContext<SignalRHub> _audioHubContext;                  // был private readonly IHubContext<SignalRHubForBlazor> _blazorHubContext;
                                                                                // проблема в этом
    private readonly IHubContext<SignalRHub, IAudioStreamReceiver> _hubContext; //

    public HubService(IHubContext<SignalRHub> audioHubContext, IHubContext<SignalRHub, IAudioStreamReceiver> hubContext)
    {
        _audioHubContext = audioHubContext;
        _hubContext = hubContext;

    }

    public async Task TransferInvokeMethod(string method)
    {
        await _audioHubContext.Clients.All.SendAsync(method); // старый метод без IAudioStreamReceiver в конструкторе
    }

    public async Task OnStopSendAudioChunk()
    {
        await _hubContext.Clients.All.OnRemoveStream();
    }

    public async Task OnStreamStarted()
    {
        await _audioHubContext.Clients.All.SendAsync("OnStreamStarted");
    }

    public async Task OnStreamStopped()
    {
        await _audioHubContext.Clients.All.SendAsync("OnStreamStopped");
    }

    public async Task OnRemoveStream()
    {
        await _audioHubContext.Clients.All.SendAsync("OnRemoveStream");
    }

    public async Task OnAudioChunk(byte[] chunk)
    {
        await _audioHubContext.Clients.All.SendAsync("OnAudioChunk", chunk);
    }

    // не знаю нужны ли будут эти методы
    public async Task NotifyAudioClientsStart()
    {
        await _hubContext.Clients.All.OnStreamStarted();
    }
    public async Task NotifyAudioClientsStop()
    {
        await _hubContext.Clients.All.OnStreamStopped();
    }

    public async Task SendAudioChunkToAll(byte[] chunk)
    {
        await _hubContext.Clients.All.OnAudioChunk(chunk);
    }
}

