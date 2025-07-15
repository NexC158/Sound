using System.Net.Sockets;
using System.Net;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks.Dataflow;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public class SignalRHub : Hub<IAudioStreamReceiver>, ISignalRHub
{
    private readonly ILogger<SignalRHub> _logger;
    private Channel<byte> _channel;

    private List<byte> _audioBuffer = new();
    private const int _packetSize = 10; 

    public SignalRHub(ILogger<SignalRHub> logger)
    {
        this._logger = logger;

        //var options = new BoundedChannelOptions(1)
        //{
        //    SingleReader = true,
        //    SingleWriter = false, // ?
        //    FullMode = BoundedChannelFullMode.Wait
        //};
        //_channel = Channel.CreateBounded<byte>(options);
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

    public async Task ReceiveAudioChunk(byte[] chunk)
    {
        Console.WriteLine($"Chunk length: {chunk.Length}");
        Console.WriteLine(string.Join(", ", chunk.Take(16))); // Выводит первые 16 элементов

        await Clients.Others.OnAudioChunk(chunk); // ретрансляция
    }

    public async Task ReceiveAudioStream(ChannelReader<byte> stream)
    {
        // this.Context.ConnectionId
        Console.WriteLine("STR: start stream");

        try
        {
            Console.WriteLine("stream content: ");
            Console.WriteLine($"stream content: {stream}");
            await foreach (var chunk in stream.ReadAllAsync())
            {
                
                    Console.Write($"{chunk}");
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

    public async Task CreateAndSendAudioChunk()
    {
        while (await _channel.Reader.WaitToReadAsync())
        {
            while (_channel.Reader.TryRead(out var b))
            {
                _audioBuffer.Add(b);
                if (_audioBuffer.Count >= _packetSize)
                {
                    var packet = _audioBuffer.Take(_packetSize).ToArray();
                    _audioBuffer.RemoveRange(0, _packetSize);
                    await Clients.Others.OnAudioChunk(packet);
                }
            }
        }
    }

    public async Task<string> GetHelloWorld()
    {
        Console.WriteLine($"Hello world: {this.Context.ConnectionId}");
        Console.WriteLine($"features: [{string.Join(", ", this.Context.Features)}]");
        return "hello word";
    }
}

