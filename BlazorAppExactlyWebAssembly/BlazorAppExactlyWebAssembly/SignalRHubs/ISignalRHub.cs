using System.Threading.Channels;
using TypedSignalR.Client;

namespace BlazorAppExactlyWebAssembly.SignalRHubs;

[Hub]
public interface ISignalRHub
{
    Task StartStreamingCommand();
    Task StopStreamingCommand();
    string GetMyConnectionId();
    Task GetBytesFromAudioStream(ChannelReader<byte> stream);

    Task CreateAndSendAudioChunk();

    Task<string> GetHelloWorld();
}

