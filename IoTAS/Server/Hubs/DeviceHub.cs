using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using IoTAS.Shared.Hubs;

namespace IoTAS.Server.Hubs
{
    public interface IDeviceHub
    {
        // empty for milestone 1
    }

    public class DeviceHub : Hub<IDeviceHub>, IDeviceHubServer
    {
        private readonly ILogger<DeviceHub> logger;

        public DeviceHub(ILogger<DeviceHub> logger)
        {
            this.logger = logger;

            logger.LogInformation("DeviceHub created");
        }

        public override async Task OnConnectedAsync()
        {
            logger.LogInformation("Device connected {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception == null)
            {
                logger.LogInformation("Device disconnected {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                logger.LogWarning(exception, "Device disconnected with exception");
            }
            await base.OnDisconnectedAsync(exception);
        }

        Task IDeviceHubServer.Heartbeat(DeviceHeartbeatInDTO deviceHeartbeatAttributes)
        {
            logger.LogInformation("Heartbeat received from Device with {DeviceId}", 
                                   deviceHeartbeatAttributes.DeviceId);

            return Task.CompletedTask;
        }

        Task IDeviceHubServer.RegisterDevice(DeviceRegistrationInDTO deviceRegistrationAttributes)
        {
            logger.LogInformation("Device registration received from Device with {DeviceId}", 
                                   deviceRegistrationAttributes.DeviceId);

            return Task.CompletedTask;
        }
    }
}
