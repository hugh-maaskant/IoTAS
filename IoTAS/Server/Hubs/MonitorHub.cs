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

        private readonly IHubsInputQueueService queueService;

        public MonitorHub(ILogger<IMonitorHub> logger, IHubsInputQueueService queueService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
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

            Request request = Request.FromInDTO(monitorRegistrationArgs);

            queueService.Enqueue(request);

            return Task.CompletedTask;
        }
    }
}
