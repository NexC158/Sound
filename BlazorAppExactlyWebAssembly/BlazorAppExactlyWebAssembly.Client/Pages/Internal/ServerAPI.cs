using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Data.Common;

namespace BlazorAppExactlyWebAssembly.Client.Pages.Internal;

public class ServerAPI : IAsyncDisposable
{
    private HubConnection _connection;
    //private NavigationManager _navigationManager;
    public ServerAPI(/*NavigationManager navigationManager*/)
    {
        //navigationManager = _navigationManager;
        // _connection = BuildAndStartConnection().Result;
    }

    public async Task<(bool isFailed, string failedReaaon)> Init()
    {
        try
        {
            _connection = await BuildAndStartConnection();
            return (false, "");
        }
        catch (Exception ex)
        {
            return (true, ex.ToString());
        }
    }

    public static async Task<HubConnection> BuildAndStartConnection()
    {
        try
        {
            var connection = new HubConnectionBuilder()
           //.WithUrl(new Uri("hubs/blazor"))
           .WithUrl("https://localhost:7069/hubs/blazor")
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

    public static string BuildAndStartConnection_test()
    {
        try
        {
            var connection = new HubConnectionBuilder()
               .WithUrl("https://localhost:7069/hubs/blazor")
               .Build();

            connection.StartAsync().Wait();
            return "ok";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnInitializedAsync: {ex.Message}");
            return ex.ToString();
        }
    }

    public async Task StartStreamingCommand()
    {
        Console.WriteLine("SignalRHubForBlazor StartStreamingCommand ... ");
        await _connection.InvokeAsync("StartStreamingCommand");
        Console.WriteLine("SignalRHubForBlazor StartStreamingCommand done");
    }
    public async Task StopStreamingCommand()
    {
        await _connection.InvokeAsync("stopTranslateAudio");
        Console.WriteLine("SignalRHubForBlazor StopStreamingCommand ");
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        var connection = _connection;
        if (connection != null)
        {
            await connection.DisposeAsync();
        }
    }
}


