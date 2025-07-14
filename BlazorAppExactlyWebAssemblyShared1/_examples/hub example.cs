#if false

[Hub]
public interface IVoiceLibraryService
{
    Task<VoiceLibraryRecord[]> GettAllRecords();

    Task<OperationResult> RemoveRecord(string recordKey);
}

[Receiver]
public interface IVoiceLibraryReceiver
{
    Task OnNewRecord(VoiceLibraryRecord record);
    Task OnRemovedRecord(string recordKey);
}



public class VoiceLibraryHub : Hub<IVoiceLibraryReceiver>, IBaseRhqHub, IVoiceLibraryService
{
    private readonly IVoiceLibraryService _service;
    private readonly ILogger<VoiceLibraryHub> _logger;

    public VoiceLibraryHub(
        VoiceLibraryService service,
        ILogger<VoiceLibraryHub> logger)
    {
        this._service = service;
        this._logger = logger;
    }

    public Task<VoiceLibraryRecord[]> GettAllRecords()
    {
        return this._service.GettAllRecords();
    }

    public Task<OperationResult> RemoveRecord(string recordKey)
    {
        return this._service.RemoveRecord(recordKey);
    }
}


public class VoiceLibraryHubPublisher : IVoiceLibraryReceiver
{
    private readonly IHubContext<VoiceLibraryHub, IVoiceLibraryReceiver> hub;
    private readonly ILogger<VoiceLibraryHubPublisher> logger;

    public VoiceLibraryHubPublisher(
        IHubContext<VoiceLibraryHub, IVoiceLibraryReceiver> hub,
        ILogger<VoiceLibraryHubPublisher> logger
        )
    {
        this.hub = hub;
        this.logger = logger;
    }

    public Task OnNewRecord(VoiceLibraryRecord record)
    {
        return hub.Clients.All.OnNewRecord(record);
    }

    public Task OnRemovedRecord(string recordKey)
    {
        return hub.Clients.All.OnRemovedRecord(recordKey);
    }
}


// для DI
// services.AddSingleton<IVoiceLibraryReceiver, VoiceLibraryHubPublisher>();
#endif
