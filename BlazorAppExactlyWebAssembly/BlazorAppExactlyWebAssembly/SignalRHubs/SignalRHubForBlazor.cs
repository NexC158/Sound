using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public class SignalRHubForBlazor : Hub
{
    private readonly HubService _hubService;

    public SignalRHubForBlazor(HubService hubService)
    {
        _hubService = hubService;
    }

    public async Task HubStartStreamingCommand()
    {
        Console.WriteLine("SignalRHubForBlazor HubStartStreamingCommand ");
        await _hubService.TransferInvokeStart(); // "SignalRHubStartStreamingCommand"
    }        
    public async Task HubStopStreamingCommand()
    {
        Console.WriteLine("SignalRHubForBlazor HubStopStreamingCommand ");
        await _hubService.TransferInvokeStop(); // "SignalRHubStopStreamingCommand"
    }

    public override Task OnConnectedAsync()
    {
        //_logger.LogTrace("Logger: connection opened: {this.Context.ConnectionId}", this.Context.ConnectionId);
        Console.WriteLine($"SignalRHubForBlazor (OnConnectedAsync) connection opened:   {this.Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        //_logger.LogTrace("Logger: connection closed: {this.Context.ConnectionId}", this.Context.ConnectionId);
        if (exception != null)
        {
            Console.WriteLine($"SignalRHubForBlazor (OnDisconnectedAsync) connection closed {this.Context.ConnectionId}  exception: {exception.Message}");
        }
        else
        {
            Console.WriteLine($"SignalRHubForBlazor (OnDisconnectedAsync) connection closed {this.Context.ConnectionId} with exception is \"{exception}\"");
        }
        return base.OnDisconnectedAsync(exception);
    }
}
