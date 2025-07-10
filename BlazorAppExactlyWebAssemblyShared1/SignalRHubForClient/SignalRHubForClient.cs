
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace BlazorAppExactlyWebAssembly.SignalRHubs
{
    public class SignalRHubForBlazor : Hub
    {
        private readonly HubConnection _connection;

        public SignalRHubForBlazor()
        {
           // _connection = new HubConnectionBuilder()
           //.WithUrl(new Uri("https://127.0.0.1:10000/audiohub"))
           //.Build();
        }

        //public async Task StartStreamingCommand()
        //{
        //    await Clients.All.SendAsync("startTranslateAudio");
        //}
        //public async Task StopStreamingCommand()
        //{
        //    await Clients.All.SendAsync("stopTranslateAudio");
        //}

        //public async Task StartConnectionAsync()
        //{
        //    if (_connection.State == HubConnectionState.Disconnected)
        //        await _connection.StartAsync();
        //}

        //public async Task StopConnectionAsync()
        //{
        //    if (_connection.State == HubConnectionState.Connected)
        //        await _connection.StopAsync();
        //}
    }
}
