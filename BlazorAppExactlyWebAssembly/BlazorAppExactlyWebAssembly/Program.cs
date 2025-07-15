using BlazorAppExactlyWebAssembly.Client.Pages;
using BlazorAppExactlyWebAssembly.Client.Pages.Internal;
using BlazorAppExactlyWebAssembly.Components;
using BlazorAppExactlyWebAssembly.SignalRHubShared2;


namespace BlazorAppExactlyWebAssembly
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddSignalR();

            builder.Services.AddTransient<ServerAPI>(); // some experiments AddTransient or AddScoped of AddSingleton
            builder.Services.AddSingleton<HubService>();
            builder.Services.AddSingleton<IAudioStreamReceiver>(sp => (IAudioStreamReceiver)sp.GetRequiredService<HubService>());

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .WithOrigins("https://localhost:7069");
                });
            });

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
            

            app.Run();
        }
    }
}