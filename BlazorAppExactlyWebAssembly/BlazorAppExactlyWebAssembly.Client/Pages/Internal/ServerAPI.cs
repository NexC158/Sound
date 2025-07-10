namespace BlazorAppExactlyWebAssembly.Client.Pages.Internal;

public class ServerAPI
{
    private readonly SignalRHubForClient _signalRHubForClient;

    public ServerAPI(

        )
    { }

        public async Task InitializeAsync()
    {
        await _signalRHubForClient.StartConnectionAsync();
    }

}


