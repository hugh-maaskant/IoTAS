using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

using IoTAS.Server.Hubs;
using IoTAS.Shared.Hubs;
using IoTAS.Server.DevicesStatusStore;

namespace IoTAS.Server.InputQueue
{
    public sealed class DispatchHandler
    {
        private readonly ILogger<DispatchHandler> logger;

        private readonly IDeviceStatusStore store;

        private readonly IHubContext<MonitorHub> monitorHub;

        // private readonly MonitorHub monitorHub;

        public DispatchHandler(
            ILogger<DispatchHandler> logger, 
            IDeviceStatusStore store,
            IHubContext<MonitorHub> monitorHub)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.monitorHub = monitorHub ?? throw new ArgumentNullException(nameof(monitorHub));
        }

        public async Task Dispatch(Request request)
        {
            if (request is null)
            {
                logger.LogWarning($"Dispatch called with request == null");
                return;
            }

            switch (request.ReceivedData)
            {
                case DevToSrvDeviceRegistrationArgs:
                    HandleDeviceRegistration(request);
                    break;
                case DevToSrvDeviceHeartbeatArgs:
                    HandleDeviceHeartbeat(request);
                    break;
                case MonToSrvRegistrationArgs:
                    HandleMonitorRegistration(request);
                    break;
                default:
                    logger.LogError($"Unknown request type : {request}");
                    break;
            }


            // just for testing the build
            await Task.Delay(1);

            return;
        }

        private void HandleDeviceRegistration(Request request)
        {
            var args = request.ReceivedData as DevToSrvDeviceRegistrationArgs;
            DeviceReportingStatus status = store.UpdateRegistration(args.DeviceId, request.ReceivedAt);
            var dto = new SrvToMonDeviceStatusArgs(
                status.DeviceId,
                status.FirstRegisteredAt,
                status.LastRegisteredAt,
                status.LastSeenAt);
            
        }


        private void HandleDeviceHeartbeat(Request request)
        {
            throw new NotImplementedException();
        }

        private void HandleMonitorRegistration(Request request)
        {
            throw new NotImplementedException();
        }

    }
}
