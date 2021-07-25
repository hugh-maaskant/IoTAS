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
            logger.LogInformation("Device connected with ConnectionId {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception == null)
            {
                logger.LogWarning("Device with ConnectionId {ConnectionId} disconnected", Context.ConnectionId);
            }
            else
            {
                logger.LogError(exception, "Device disconnected with exception");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task RegisterDeviceClient(DevToSrvDeviceRegistrationDto deviceRegistrationAttributes)
        {
            logger.LogInformation("Device registration received from Device with DeviceId {DeviceId} on Connection {ConnectionId}",
                                   deviceRegistrationAttributes.DeviceId, Context.ConnectionId);

            Request request = Request.FromClientCall(Context.ConnectionId, deviceRegistrationAttributes);

            queueService.Enqueue(request);

            return Task.CompletedTask;
        }

        public Task ReceiveDeviceHeartbeat(DevToSrvDeviceHeartbeatDto deviceHeartbeatAttributes)
        {
            logger.LogInformation("ReceiveDeviceHeartbeat received from Device with DeviceId {DeviceId} on Connection {ConnectionId}", 
                                   deviceHeartbeatAttributes.DeviceId, Context.ConnectionId);

            Request request = Request.FromClientCall(Context.ConnectionId, deviceHeartbeatAttributes);

            queueService.Enqueue(request);
            return Task.CompletedTask; ;
        }        
    }
}
