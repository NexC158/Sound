using System.Threading.Channels;
using TypedSignalR.Client;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

[Hub]
public interface ISignalRHub
{
    string GetMyConnectionId();

    //Task CreateAndSendAudioChunk();
    //Task ReceiveAudioStream(ChannelReader<byte> stream);
}

