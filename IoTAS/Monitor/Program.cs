//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//
using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

using Serilog;

namespace IoTAS.Monitor
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.BrowserConsole()
                .CreateLogger();
            Log.Information("Monitor started ...");

            try
            {
                var builder = WebAssemblyHostBuilder.CreateDefault(args);
                Log.Information($"Base address is {builder.HostEnvironment.BaseAddress}");

                builder.RootComponents.Add<App>("#app");
                // builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

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
