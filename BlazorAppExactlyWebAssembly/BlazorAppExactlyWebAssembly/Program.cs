using BlazorAppExactlyWebAssembly.Client.Pages;
using BlazorAppExactlyWebAssembly.Client.Pages.Internal;
using BlazorAppExactlyWebAssembly.Components;
using BlazorAppExactlyWebAssembly.SignalRHubs;


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

            builder.Services.AddTransient<ServerAPI>(); // some experiments AddTransient or AddScoped

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

/*������ ������� ��� �����: �������� ���������� ����� SignalRHubForBlazor �� ���� /hubs/blazor ��� �������� ��������. ��� ���������� ������ ���� ���� ������� ��������. ����� ����� � ������� �� ������ 
<button class= "btn btn-primary" @onclick = "SoundStreaming" >@(isActive ? "Stop translate" : "Start translate") </ button >
������ ��������� ������ ����������: app.MapHub<SignalRHub>("/hubs/audiohub"); ������� ������ ����� ���������� ���� � ���������*/
