using Concentus;
using Concentus.Structs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public class SignalRHub : Hub<IAudioStreamReceiver>, ISignalRHub
{
    private readonly ILogger<SignalRHub> _logger;

    private readonly Channel<byte> _channel;

    //private Channel<byte[]> _channel;

    private List<byte> _audioBuffer = new();
    private const int _packetSize = 127;

    private OpusDecoder _opusDecoder;
    private const int _pcmSamples = 800; // 8000Hz * 0.1ms

    public SignalRHub(ILogger<SignalRHub> logger)
    {
        this._logger = logger;

        _channel = Channel.CreateUnbounded<byte>();
        //_opusDecoder = (OpusDecoder)OpusCodecFactory.CreateDecoder(8000, 1);

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

    public async Task ReceiveAudioStream(ChannelReader<byte> stream)
    {
        Console.WriteLine("STR: start stream");
        try
        {
            await foreach (var b in stream.ReadAllAsync())
            {
                _channel.Writer.TryWrite(b);
            }
            Console.WriteLine("STR: stop stream");
        }
        catch (Exception ex)
        {
            Console.WriteLine("STR: failed stream");
            Console.WriteLine($"ReceiveAudioStream {ex.Message}");
        }

    }

    public async Task IsHaveBytes()
    {
        Console.WriteLine("IsHaveBytes");
        var sum = 0;
        var i = 0;
        var countOfBytes = 2000;

        await foreach (var b in _channel.Reader.ReadAllAsync())
        {
            sum += b;
            i++;
            Console.Write($"{b} ");

            if (i == countOfBytes)
            {
                Console.WriteLine();
                Console.WriteLine($"{sum}       {sum / countOfBytes}");
                Console.WriteLine();
                sum = 0;
                i = 0;
            }
        }
    }

    public async Task CreateAndSendAudioChunk()
    {
        Console.WriteLine("CreateAndSendAudioChunk");
        await foreach (var b in _channel.Reader.ReadAllAsync())
        {
            try
            {
                _audioBuffer.Add(b);
                //Console.WriteLine($"    {b}     {_audioBuffer.Count}");
                if (_audioBuffer.Count == _pcmSamples)
                {
                    var packet = _audioBuffer.ToArray();
                    _audioBuffer.Clear(); // RemoveRange(0, _packetSize)

                    var pcmBuf = new short[_pcmSamples];
                    int samplesDecoded = _opusDecoder.Decode(packet.AsSpan(), pcmBuf.AsSpan(), _pcmSamples, false);
                    // декодированный 
                    byte[] pcmBytes = new byte[samplesDecoded * 2]; // short = 2 байта
                    Buffer.BlockCopy(pcmBuf, 0, pcmBytes, 0, pcmBytes.Length);
                    //foreach (var pcm in pcmBytes)
                    //{
                    //    Console.Write(pcm.ToString());
                    //}
                    //Console.WriteLine();

                    //await Clients.Others.OnAudioChunk(packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}

