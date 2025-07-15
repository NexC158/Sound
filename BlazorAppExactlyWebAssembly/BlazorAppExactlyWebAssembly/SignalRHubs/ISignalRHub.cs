using System.Threading.Channels;
using TypedSignalR.Client;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared2;

[Hub]
public interface ISignalRHub
{
    //Task StartStreamingCommand();
    //Task StopStreamingCommand();
   // string GetMyConnectionId();
    Task CreateAndSendAudioChunk();

    Task ReceiveAudioStream(ChannelReader<byte> stream);

    Task<string> GetHelloWorld();
}

