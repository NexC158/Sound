using System.Net.Sockets;
using System.Net;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks.Dataflow;

namespace BlazorAppExactlyWebAssembly.SignalRHubs;

public class SignalRHub : Hub, ISignalRHub
{
    private readonly ILogger<SignalRHub> _logger;
    private Channel<byte> _channel;

    private List<byte> _audioBuffer = new();
    private const int _packetSize = 10; 

    public SignalRHub(ILogger<SignalRHub> logger)
    {
        this._logger = logger;

        var options = new BoundedChannelOptions(1)
        {
            SingleReader = true,
            SingleWriter = false, // ?
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<byte>(options);
    }
    public async Task StartStreamingCommand()
    {
        await Clients.All.SendAsync("startTranslateAudio");
    }

    public async Task StopStreamingCommand()
    {
        await Clients.All.SendAsync("stopTranslateAudio");
    }

    public string GetMyConnectionId()
    {
        return Context.ConnectionId;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogTrace("Logger: connection opened: {this.Context.ConnectionId}", this.Context.ConnectionId);
        Console.WriteLine($"connection (OnConnectedAsync) opened: {this.Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogTrace("Logger: connection closed: {this.Context.ConnectionId}", this.Context.ConnectionId);
        Console.WriteLine($"connection (OnDisconnectedAsync) closed: {this.Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }


    public async Task GetBytesFromAudioStream(ChannelReader<byte> stream)
    {
        // this.Context.ConnectionId
        Console.WriteLine("STR: start stream");
        var writer = _channel.Writer;
        try
        {
            Console.WriteLine("stream content: ");
            await foreach (var b in stream.ReadAllAsync())
            {
                writer.TryWrite(b);
                // foreach (var b in chunk)
                {
                    Console.Write(b);
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

    public async Task CreateAndSendAudioChunk()
    {
        var reader = _channel.Reader;

        byte[] packet = new byte[_packetSize];
        try
        {
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var b))
                {
                    _audioBuffer.Add(b);
                    if (_audioBuffer.Count == _packetSize)
                    {
                        packet = _audioBuffer.Take(_packetSize).ToArray();
                        _logger.LogTrace("Logger: AudioChunk: {this.Context.ConnectionId}, Размер пакета: {packet.Length} байт", this.Context.ConnectionId, packet.Length);
                        _audioBuffer.RemoveRange(0, _packetSize);

                        var preview = string.Join(", ", packet.Take(10));
                        await Clients.Others.SendAsync("ReceiveAudioChunk", packet);
                    }
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
        
    }

    public async Task<string> GetHelloWorld()
    {
        Console.WriteLine($"Hello world: {this.Context.ConnectionId}");
        Console.WriteLine($"features: [{string.Join(", ", this.Context.Features)}]");
        return "hello word";
    }
}

