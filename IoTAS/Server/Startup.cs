using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

using Serilog;

using IoTAS.Shared.Hubs;
using IoTAS.Server.InputQueue;
using IoTAS.Server.DevicesStatusStore;

namespace IoTAS.Server
{
    public class Startup
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
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSignalR();

            services.AddSingleton<IHubsInputQueueService, HubsInputQueueService>();
            services.AddSingleton<IDeviceStatusStore, VolatileDeviceStatusStore>();

            services.AddHostedService<InputDispatcherHostedService>();
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
                endpoints.MapControllers();
                endpoints.MapHub<Hubs.DeviceHub>(IDeviceHubServer.path);
                endpoints.MapHub<Hubs.MonitorHub>(IMonitorHubServer.path);
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
