using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using IoTAS.Shared.Hubs;
using IoTAS.Server.InputQueue;

namespace IoTAS.Server.Hubs
{
    public interface IMonitorHub
    {
        // empty for milestone 1
    }

    public class MonitorHub : Hub<IMonitorHub>
    {
        private readonly ILogger<IMonitorHub> logger;

        public MonitorHub(ILogger<IMonitorHub> logger)
        {
            this.logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            logger.LogInformation("Monitor connected with ConnectionId {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception == null)
            {
                logger.LogInformation("Monitor with ConnectionId {ConnectionId} disconnected", Context.ConnectionId);
            }
            else
            {
                logger.LogWarning(exception, "Monitor disconnected with exception");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task RegisterMonitorAsync(MonToSrvRegistrationArgs monitorRegistrationArgs)
        {
            logger.LogInformation("Monitor registration received on Connection {ConnectionId}", Context.ConnectionId);

            // ToDo: nothing ???

            return Task.CompletedTask;
        }
    }
}
