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
    public interface IDeviceHub
    {
        // empty for milestone 1
    }

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
                logger.LogInformation("Device with ConnectionId {ConnectionId} disconnected", Context.ConnectionId);
            }
            else
            {
                logger.LogWarning(exception, "Device disconnected with exception");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task RegisterDeviceAsync(DevToSrvDeviceRegistrationArgs deviceRegistrationAttributes)
        {
            logger.LogInformation("Device registration received from Device with DeviceId {DeviceId} on Connection {ConnectionId}",
                                   deviceRegistrationAttributes.DeviceId, Context.ConnectionId);

            Request request = Request.FromInDTO(deviceRegistrationAttributes);

            queueService.Enqueue(request);

            return Task.CompletedTask;
        }

        public Task HandleHeartbeatAsync(DevToSrvDeviceHeartbeatArgs deviceHeartbeatAttributes)
        {
            logger.LogInformation("HandleHeartbeatAsync received from Device with DeviceId {DeviceId} on Connection {ConnectionId}", 
                                   deviceHeartbeatAttributes.DeviceId, Context.ConnectionId);

            Request request = Request.FromInDTO(deviceHeartbeatAttributes);

            queueService.Enqueue(request);
            return Task.CompletedTask; ;
        }        
    }
}
