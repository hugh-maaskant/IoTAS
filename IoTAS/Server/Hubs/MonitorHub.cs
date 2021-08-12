using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


using IoTAS.Shared.Hubs;
using IoTAS.Server.InputQueue;

namespace IoTAS.Server.Hubs
{
    /// <summary>
    /// Typesafe SignalR Hub for Monitor Clients
    /// </summary>
    public class MonitorHub : Hub<IMonitorHub>, IMonitorHubServer
    {
        private readonly ILogger<IMonitorHub> logger;

        private readonly IHubsInputQueue queueService;

        public MonitorHub(ILogger<IMonitorHub>? logger, IHubsInputQueue queueService)
        {
            this.logger = logger ?? NullLogger<IMonitorHub>.Instance;
            this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
        }

        public override async Task OnConnectedAsync()
        {
            logger.LogInformation(
                nameof(OnConnectedAsync) + " - " +
                "Monitor connected on ConnectionId {ConnectionId}",
                Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            if (e == null)
            {
                // It's OK for Monitors to go away from their web-page
                logger.LogInformation(
                    nameof(OnDisconnectedAsync) + " - " +
                    "Monitor on ConnectionId {ConnectionId} disconnected", 
                    Context.ConnectionId);
            }
            else
            {
                logger.LogError(
                    e,
                    nameof(OnDisconnectedAsync) + " - " +
                    "Monitor on ConnectionId {ConnectionId} disconnected with exception", 
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
                "Monitor registration received on ConnectionId {ConnectionId}",
                Context.ConnectionId);

            Request request = Request.FromClientCall(Context.ConnectionId, monitorRegistrationDto);

            queueService.Enqueue(request);

            return Task.CompletedTask;
        }
    }
}
