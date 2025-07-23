namespace BlazorAppExactlyWebAssembly.SignalRHubShared;

public interface IAudioStreamReceiver
{
    Task OnCustomCommandStart();
    Task OnCustomCommandStop();
}
