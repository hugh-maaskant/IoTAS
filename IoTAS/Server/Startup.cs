//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using IoTAS.Server.Hubs;
using IoTAS.Server.InputQueue;
using IoTAS.Shared.DevicesStatusStore;
using IoTAS.Shared.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace IoTAS.Server;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        Log.Information(nameof(Startup) + ": " + nameof(ConfigureServices));
        // services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddSignalR();

        services.AddSingleton<IHubsInputQueueService, HubsInputQueueService>();
        services.AddSingleton<IDeviceStatusStore, VolatileDeviceStatusStore>();

        services.AddHostedService<InputProcessorHostedService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Log.Information(nameof(Startup) + ": " + nameof(Configure));
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days.
            // You may want to change this for production scenarios,
            // see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseSerilogRequestLogging();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            // endpoints.MapControllers();
            endpoints.MapHub<DeviceHub>(IDeviceHubServer.Path);
            endpoints.MapHub<MonitorHub>(IMonitorHubServer.Path);
            endpoints.MapFallbackToFile("index.html");
        });
    }
}