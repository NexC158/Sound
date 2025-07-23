using BlazorAppExactlyWebAssembly.Client.Pages.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorAppExactlyWebAssembly.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddSingleton<ServerAPI>();

        var app = builder.Build();

        app.Services.GetRequiredService<ServerAPI>();

        Console.WriteLine("in client main 2");

        Console.WriteLine("in client main 3");

        await app.RunAsync();
    }
}
