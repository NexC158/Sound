using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorAppExactlyWebAssembly.SignalRHubForClient;

public class SignalRHubForClient : Hub
{
    private readonly HubConnection _connection;

    public SignalRHubForClient(NavigationManager navigationManager)
    {
        _connection = new HubConnectionBuilder()
           .WithUrl(navigationManager.ToAbsoluteUri("/audiohub"))
           .Build();
    }

    public async Task StartStreamingCommand()
    {
        await Clients.All.SendAsync("startTranslateAudio");
    }        
    public async Task StopStreamingCommand()
    {
        await Clients.All.SendAsync("stopTranslateAudio");
    }

    public async Task StartConnectionAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
            await _connection.StartAsync();
    }

    public async Task StopConnectionAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            await _connection.StopAsync();
    }
}
