using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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

                Log.Information($"Baseaddress is {builder.HostEnvironment.BaseAddress}");

                builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

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
