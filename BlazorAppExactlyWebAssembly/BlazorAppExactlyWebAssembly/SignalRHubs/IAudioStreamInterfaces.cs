using TypedSignalR.Client;

namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public interface IAudioStreamService
{
    Task<SignalRHub[]> GetAllAudioStreams();

    Task RemoveStream(); // string streamId
}

[Receiver]
public interface IAudioStreamReceiver
{
    //Task OnStreamStarted();
    //Task OnStreamStopped();
    //Task OnRemoveStream();
    //Task OnAudioChunk(byte[] chunk);
    Task OnCustomCommandStart();
    Task OnCustomCommandStop();

}
