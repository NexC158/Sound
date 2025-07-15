using BlazorAppExactlyWebAssembly.Client.Pages.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorAppExactlyWebAssembly.Client;



public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("in client main");
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddSingleton<ServerAPI>(); // some experiments AddTransient or AddScoped or AddSingleton

        var app = builder.Build();

        app.Services.GetRequiredService<ServerAPI>();

        Console.WriteLine("in client main 2");
        //var apiInitRes = await api.Init();
        Console.WriteLine("in client main 3");
        //if (apiInitRes.isFailed)
        //{
        //    _dbg = apiInitRes.failedReaaon;
        //    Console.WriteLine(apiInitRes.failedReaaon);
        //}

        await app.RunAsync();
    }
}
