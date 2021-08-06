using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

using IoTAS.Shared.Hubs;

using Serilog;

namespace IoTAS.Monitor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.BrowserConsole()
                .CreateLogger();
            Log.Information("Monitor started ...");

            try
            {
                var builder = WebAssemblyHostBuilder.CreateDefault(args);
                builder.RootComponents.Add<App>("#app");

                Log.Information($"Base address is {builder.HostEnvironment.BaseAddress}");
                // builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

                Log.Information($"Using MonitorHub at {IMonitorHubServer.path}");
                // Add HubConnection to Services so we can access the MonitorHub from any component
                // by injectiong the HubConnection. To avoid delaying startup, do not yet start the
                // connection here.
                builder.Services.AddSingleton<HubConnection>(sp =>
                {
                    var navigationManager = sp.GetRequiredService<NavigationManager>();
                    return new HubConnectionBuilder()
                    .WithUrl(navigationManager.ToAbsoluteUri(IMonitorHubServer.path))
                    .Build();
                }
                );

                await builder.Build().RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An exception occurred while creating the WASM host");
                throw;
            }
        }
    }
}
