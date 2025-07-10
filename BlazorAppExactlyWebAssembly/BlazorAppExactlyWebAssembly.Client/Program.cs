using BlazorAppExactlyWebAssembly.Client.Pages.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorAppExactlyWebAssembly.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddSingleton<ServerAPI>();

            await builder.Build().RunAsync();
        }
    }
}
