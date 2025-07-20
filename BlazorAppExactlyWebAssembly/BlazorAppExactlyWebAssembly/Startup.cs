using BlazorAppExactlyWebAssembly.Client.Pages.Internal;
using BlazorAppExactlyWebAssembly.SignalRHubShared;

namespace BlazorAppExactlyWebAssembly;

public class Startup
{
    private readonly IConfiguration _configuration;
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddRazorComponents().AddInteractiveWebAssemblyComponents();

        services.AddSignalR().AddMessagePackProtocol(/*options =>  //  AddJsonProtocol()
        {
            options.SerializerOptions = MessagePackSerializerOptions.Standard
                .WithResolver(new CustomResolver())
                .WithSecurity(MessagePackSecurity.UntrustedData);
        }*/);

        services.AddTransient<ServerAPI>();

        services.AddSingleton<HubService>();

        services.AddSingleton<IAudioStreamReceiver>(sp => (IAudioStreamReceiver)sp.GetRequiredService<HubService>());

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .WithOrigins("https://localhost:7069");
            });
        });
    }
}
