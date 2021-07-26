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
    public class DeviceHub : Hub<IDeviceHub>, IDeviceHubServer
    {
        private readonly ILogger<DeviceHub> logger;

        private readonly IHubsInputQueueService queueService;

        public DeviceHub(ILogger<DeviceHub> logger, IHubsInputQueueService queueService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
        }

        public override async Task OnConnectedAsync()
        {
            logger.LogInformation(
                nameof(OnConnectedAsync) + " - " +
                "Device connected on ConnectionId {ConnectionId}", 
                Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception == null)
            {
                logger.LogWarning(
                    nameof(OnDisconnectedAsync) +" - " +
                    "Device on ConnectionId {ConnectionId} disconnected", 
                    Context.ConnectionId);
            }
            else
            {
                logger.LogError(
                    exception,
                    nameof(OnDisconnectedAsync) + " - " + 
                    "Device on ConnectionId {ConnectionId} disconnected", 
                    Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task RegisterDeviceClient(DevToSrvDeviceRegistrationDto dtoIn)
        {
            logger.LogInformation(
                nameof(RegisterDeviceClient) + " - " +
                "Device registration received from Device {DeviceId} on ConnectionId {ConnectionId}",
                dtoIn.DeviceId, Context.ConnectionId);

            Request request = Request.FromClientCall(Context.ConnectionId, dtoIn);

            queueService.Enqueue(request);

            return Task.CompletedTask;
        }

        public Task ReceiveDeviceHeartbeat(DevToSrvDeviceHeartbeatDto dtoIn)
        {
            logger.LogInformation(
                nameof(ReceiveDeviceHeartbeat) + " - " +
                "Heartbeat received from DeviceId {DeviceId} on ConnectionId {ConnectionId}", 
                dtoIn.DeviceId, Context.ConnectionId);

            Request request = Request.FromClientCall(Context.ConnectionId, dtoIn);

            queueService.Enqueue(request);

            return Task.CompletedTask; ;
        }        
    }
}
