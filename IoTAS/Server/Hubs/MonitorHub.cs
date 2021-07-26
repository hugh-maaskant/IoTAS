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
    public class MonitorHub : Hub<IMonitorHub>, IMonitorHubServer
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
            logger.LogInformation(
                nameof(OnConnectedAsync) + " - " +
                "Monitor connected on Connection {ConnectionId}",
                Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            if (e == null)
            {
                // It's OK for Monitors to go awai from their web-page
                logger.LogInformation(
                    nameof(OnDisconnectedAsync) + " - " +
                    "Monitor on Connection {ConnectionId} disconnected", 
                    Context.ConnectionId);
            }
            else
            {
                logger.LogError(
                    e,
                    nameof(OnDisconnectedAsync) + " - " +
                    "Monitor on Connection {ConnectionId} disconnected with exception", 
                    Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(e);
        }

        //
        // Called by the Hub on behalf of the Monitor Client call ...
        //
        public Task RegisterMonitorClient(MonToSrvRegistrationDto monitorRegistrationDto)
        {
            logger.LogInformation(
                nameof(RegisterMonitorClient) + " - " +
                "Monitor registration received on Connection {ConnectionId}",
                Context.ConnectionId);

            Request request = Request.FromClientCall(Context.ConnectionId, monitorRegistrationDto);

            queueService.Enqueue(request);

            return Task.CompletedTask;
        }
    }
}
