using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.SignalR;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public class SignalRHub : Hub<IAudioStreamReceiver>, ISignalRHub
{
    private readonly ILogger<SignalRHub> _logger;

    private readonly Channel<byte> _channel;

    //private Channel<byte[]> _channel;

    private List<byte> _audioBuffer = new();
    private const int _packetSize = 10;

    public SignalRHub(ILogger<SignalRHub> logger)
    {
        this._logger = logger;
        _channel = Channel.CreateUnbounded<byte>();
        //
        Task.Run(IsHaveBytes);
    }

    public async Task OnStreamStarted()
    {
        await Clients.All.OnStreamStarted();
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

    public async Task ReceiveAudioChunk(ChannelReader<byte[]> chunkStream)
    {
        Console.WriteLine("STR: start stream");

        try
        {
            Console.WriteLine("stream content: ");
            await foreach (var chunkBytes in chunkStream.ReadAllAsync())
            {
                Console.Write($" {chunkBytes.Length}");

                foreach (var baytik in chunkBytes)
                {
                    _channel.Writer.TryWrite(baytik);
                }
            }

            Console.WriteLine("STR: stop stream");
        }
        catch (Exception ex)
        {
            Console.WriteLine("STR: failed stream");
            Console.WriteLine(ex.ToString());
        }
    }

    public async Task ReceiveAudioStream(ChannelReader<byte> stream)
    {
        // this.Context.ConnectionId
        Console.WriteLine("STR: start stream");
        try
        {
            Console.WriteLine("stream content: ");


            await foreach (var chunk in stream.ReadAllAsync())
            {
                _channel.Writer.TryWrite(chunk);
            }

            Console.WriteLine("STR: stop stream");
        }
        catch (Exception ex)
        {
            Console.WriteLine("STR: failed stream");
            Console.WriteLine(ex.ToString());
        }
    }

    public async Task IsHaveBytes()
    {

        var sum = 0;
        var i = 0;
        var countOfBytes = 2000;
        await foreach (var b in _channel.Reader.ReadAllAsync())
        {
            sum += b;
            i++;
            if (i == countOfBytes)
            {
                Console.WriteLine($"{sum} {countOfBytes} {sum/countOfBytes}");
                sum = 0;
                i = 0;
            }
        }

    }

    //public async Task CreateAndSendAudioChunk()
    //{
    //    while (await _channel.Reader.WaitToReadAsync())
    //    {
    //        while (_channel.Reader.TryRead(out var b))
    //        {
    //            _audioBuffer.Add(b);
    //            if (_audioBuffer.Count >= _packetSize)
    //            {
    //                var packet = _audioBuffer.Take(_packetSize).ToArray();
    //                _audioBuffer.RemoveRange(0, _packetSize);
    //                await Clients.Others.OnAudioChunk(packet);
    //            }
    //        }
    //    }
    //}
}

