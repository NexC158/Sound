namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public interface ISignalRHub
{
    string GetMyConnectionId();

    //Task CreateAndSendAudioChunk();
    //Task ReceiveAudioStream(ChannelReader<byte> stream);
}

