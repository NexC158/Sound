using Concentus;
using Concentus.Structs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public class SignalRHub : Hub<IAudioStreamReceiver>
{
    private readonly ILogger<SignalRHub> _logger;

    public SignalRHub(ILogger<SignalRHub> logger)
    {
        _logger = logger;
    }

    public string GetMyConnectionId()
    {
        return Context.ConnectionId;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogTrace("Logger: connection opened: {this.Context.ConnectionId}", this.Context.ConnectionId);
        Console.WriteLine($"SignalRHub connection (OnConnectedAsync) opened: {this.Context.ConnectionId}");

        // await Clients.Others.OnStreamStarted(); // уведомление другим клиентам что кто-то подключился (может вообще не нужно)

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogTrace("Logger: connection closed: {this.Context.ConnectionId}", this.Context.ConnectionId);
        Console.WriteLine($"SignalRHub connection (OnDisconnectedAsync) closed: {this.Context.ConnectionId}");

        // await Clients.Others.OnStreamStopped(); // уведомление другим клиентам что кто-то отключился (может вообще не нужно)

        return base.OnDisconnectedAsync(exception);
    }
}