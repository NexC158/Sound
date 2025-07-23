using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Data.Common;

namespace BlazorAppExactlyWebAssembly.Client.Pages.Internal;

public class ServerAPI : IAsyncDisposable
{
    private HubConnection _connection;

    private Timer ?_keepAlive;

    public ServerAPI()
    {
        _connection = new HubConnectionBuilder()
          .WithUrl("https://localhost:7069/hubs/blazor")
          .WithAutomaticReconnect()
          .Build();
    }
    public async Task<HubConnection> InitializeAsync()
    {
        var connection = _connection;
        if (connection.State == HubConnectionState.Disconnected)
        {
            await connection.StartAsync();
        }
        KeepAliveTimer();
        return connection;
    }

    public async Task StartStreamingCommand()
    {
        Console.WriteLine("ServerAPI StartStreamingCommand ... ");
        await _connection.InvokeAsync("HubStartStreamingCommand");
        Console.WriteLine("ServerAPI StartStreamingCommand done");
    }
    public async Task StopStreamingCommand()
    {
        Console.WriteLine("ServerAPI StopStreamingCommand ... ");
        await _connection.InvokeAsync("HubStopStreamingCommand");
        Console.WriteLine("ServerAPI StopStreamingCommand done");
    }

    private void KeepAliveTimer() // Сделал потому что начал ловить ошибку и предупреждение, из-за того, что соединение простаивало
    {
        _keepAlive = new Timer(async _ =>
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _connection.InvokeAsync("Pig");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ServerAPI KeepAliveTimer: {ex.Message}");
                }
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        var connection = _connection;

        Console.WriteLine($"ServerAPI async ValueTask IAsyncDisposable.DisposeAsync() {connection}");

        if (connection != null)
        {
            await connection.DisposeAsync();
            _keepAlive!.Dispose();
        }
    }
}



//_connection.StartAsync().Wait();

//.WithUrl("https://localhost:7069/hubs/blazor") //_navigationManager.ToAbsoluteUri("/hubs/blazor").WithAutomaticReconnect()
//_connection = BuildAndStartConnection().Result;

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