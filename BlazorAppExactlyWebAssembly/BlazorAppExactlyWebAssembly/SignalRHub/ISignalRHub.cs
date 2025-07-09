using System.Threading.Channels;
using TypedSignalR.Client;

namespace BlazorAppExactlyWebAssembly.SignalRHubServer;
[Hub]
public interface ISignalRHub
{
    Task StartStreamingCommand(string connectionId);
    Task StopStreamingCommand(string connectionId);
    string GetMyConnectionId();
    Task GetBytesFromAudioStream(ChannelReader<byte> stream);

    Task CreateAndSendAudioChunk();

    Task<string> GetHelloWorld();
}

