using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace BlazorAppExactlyWebAssembly.SignalRHubs;

public class SignalRHubForBlazor : Hub
{
    private readonly HubConnection _connection;

    public SignalRHubForBlazor()
    {
        _connection = new HubConnectionBuilder()
           .WithUrl(new Uri("https://127.0.0.1:10000/audiohub"))
           .Build();

    }

    public async Task StartStreamingCommand()
    {
        Console.WriteLine("SignalRHubForBlazor StartStreamingCommand ");
        //await _connection.InvokeAsync("startTranslateAudio");
        //await Clients.All.SendAsync("startTranslateAudio");
    }        
    public async Task StopStreamingCommand()
    {
        Console.WriteLine("SignalRHubForBlazor StopStreamingCommand ");
        //await Clients.All.SendAsync("stopTranslateAudio");
    }
}
