using BlazorAppExactlyWebAssembly.Client.Pages.Internal;
using BlazorAppExactlyWebAssembly.Components;
using BlazorAppExactlyWebAssembly.SignalRHubShared;


namespace BlazorAppExactlyWebAssembly
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Startup.ConfigureServices(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseCors();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);


            app.MapHub<SignalRHub>("/hubs/audiohub");
            app.MapHub<SignalRHubForBlazor>("/hubs/blazor");

            app.MapControllers();


            app.Run();
        }
    }
}