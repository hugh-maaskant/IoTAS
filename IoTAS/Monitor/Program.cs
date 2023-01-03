//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Serilog;
using Serilog.Events;

namespace IoTAS.Monitor;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
                

            builder.RootComponents.Add<App>("#app");
            // builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();

            // Must pass the IJSRuntime to Serilog to avoid exception
            // See https://github.com/dotnet/aspnetcore/issues/45536 

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.BrowserConsole(
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    outputTemplate: "{Level:u3}-{Message:lj}{NewLine}{Exception}",
                    CultureInfo.InvariantCulture,
                    null,
                    host.Services.GetRequiredService<IJSRuntime>())
                .CreateLogger();

            Log.Information("Monitor started ...");
            Log.Information($"Base address is {builder.HostEnvironment.BaseAddress}");

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An exception occurred while creating the WASM host");
            throw;
        }
    }
}