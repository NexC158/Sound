using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Data.Common;

namespace BlazorAppExactlyWebAssembly.Client.Pages.Internal;

public class ServerAPI : IAsyncDisposable
{
    private readonly HubConnection _connection;
    //private NavigationManager _navigationManager;
    public ServerAPI(/*NavigationManager navigationManager*/)
    {
        //navigationManager = _navigationManager;
        _connection = BuildAndStartConnection().Result;
    }

    public static async Task<HubConnection> BuildAndStartConnection()
    {
        try
        {
            var connection = new HubConnectionBuilder()
           .WithUrl(new Uri("/SignalRHubForBlazor"))
           .Build();

            await connection.StartAsync();
            return connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnInitializedAsync: {ex.Message}");
            throw;
        }
    }

    public async Task StartStreamingCommand()
    {
        await _connection.InvokeAsync("startTranslateAudio");
        Console.WriteLine("SignalRHubForBlazor StartStreamingCommand ");
    }
    public async Task StopStreamingCommand()
    {
        await _connection.InvokeAsync("stopTranslateAudio");
        Console.WriteLine("SignalRHubForBlazor StopStreamingCommand ");
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}


