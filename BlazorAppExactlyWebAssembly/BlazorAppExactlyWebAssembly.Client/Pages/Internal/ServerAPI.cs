using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Data.Common;

namespace BlazorAppExactlyWebAssembly.Client.Pages.Internal;

public class ServerAPI : IAsyncDisposable
{
    private HubConnection _connection;

    private readonly NavigationManager _navigationManager;
    public ServerAPI(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

        _connection = new HubConnectionBuilder()
          .WithUrl("https://localhost:7069/hubs/blazor")
          .Build();

        _connection.On("")

        //_connection.StartAsync().Wait();

        //.WithUrl("https://localhost:7069/hubs/blazor") //_navigationManager.ToAbsoluteUri("/hubs/blazor").WithAutomaticReconnect()
        //_connection = BuildAndStartConnection().Result;
    }

    public async Task<HubConnection> InitializeAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
        }

        return _connection;
    }

    //public async Task<(bool isFailed, string failedReaaon)> Init()
    //{
    //    try
    //    {
    //        _connection = await InitializeAsync();
    //        return (false, "");
    //    }
    //    catch (Exception ex)
    //    {
    //        return (true, ex.ToString());
    //    }
    //}

    //public static async Task<HubConnection> BuildAndStartConnection()
    //{
    //    try
    //    {
    //        var connection = new HubConnectionBuilder()
    //       //.WithUrl(new Uri("hubs/blazor"))
    //       .WithUrl("https://localhost:7069/hubs/blazor")
    //       .Build();

    //        await connection.StartAsync();
    //        return connection;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"BuildAndStartConnection: {ex.Message}");
    //        throw;
    //    }
    //}

    //public static string BuildAndStartConnection_test()
    //{
    //    try
    //    {
    //        var connection = new HubConnectionBuilder()
    //           .WithUrl("https://localhost:7069/hubs/blazor")
    //           .Build();

    //        connection.StartAsync().Wait();

    //        if (connection.State == HubConnectionState.Connected)
    //        {
    //            return "ok";
    //        }
    //        else return "notOk";
            
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"OnInitializedAsync: {ex.Message}");
    //        return ex.ToString();
    //    }
    //}

    public async Task StartStreamingCommand()
    {
        Console.WriteLine("SignalRHubForBlazor HubStartStreamingCommand ... ");
        await _connection.InvokeAsync("HubStartStreamingCommand");
        Console.WriteLine("SignalRHubForBlazor HubStartStreamingCommand done");
    }
    public async Task StopStreamingCommand()
    {
        Console.WriteLine("SignalRHubForBlazor HubStopStreamingCommand ... ");
        await _connection.InvokeAsync("HubStopStreamingCommand");
        Console.WriteLine("SignalRHubForBlazor HubStopStreamingCommand done");
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        var connection = _connection;

        Console.WriteLine($"ServerAPI async ValueTask IAsyncDisposable.DisposeAsync() {connection}");

        if (connection != null)
        {
            await connection.DisposeAsync();
        }
    }
}


