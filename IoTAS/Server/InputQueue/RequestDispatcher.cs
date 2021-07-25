using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

using IoTAS.Shared.Hubs;
using IoTAS.Server.Hubs;
using IoTAS.Server.DevicesStatusStore;

namespace IoTAS.Server.InputQueue
{
    public sealed class RequestDispatcher
    {
        private readonly ILogger<RequestDispatcher> logger;

        private readonly IDeviceStatusStore store;

        private readonly IHubContext<MonitorHub, IMonitorHub> monitorHub;

        public RequestDispatcher(
            ILogger<RequestDispatcher> logger, 
            IDeviceStatusStore store,
            IHubContext<MonitorHub, IMonitorHub> monitorHub)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.monitorHub = monitorHub ?? throw new ArgumentNullException(nameof(monitorHub));

            logger.LogDebug("Created");
        }

        public async Task DispatchRequest(Request request)
        {
            if (request is null)
            {
                logger.LogWarning($"{nameof(DispatchRequest)} called with request == null");
                return;
            }

            switch (request.ReceivedData)
            {
                case DevToSrvDeviceRegistrationDto:
                    await HandleDeviceRegistration(request);
                    break;
                case DevToSrvDeviceHeartbeatDto:
                    await HandleDeviceHeartbeat(request);
                    break;
                case MonToSrvRegistrationDto:
                    await HandleMonitorRegistration (request);
                    break;
                default:
                    logger.LogError($"Unknown request type : {request}");
                    break;
            }
        }

        private async Task HandleDeviceRegistration(Request request)
        {
            var args = request.ReceivedData as DevToSrvDeviceRegistrationDto;

            if (args is null)
            {
                logger.LogError($"{nameof(HandleDeviceRegistration)} - Request is not {nameof(DevToSrvDeviceRegistrationDto)}: {request}");
                return;
            }

            logger.LogDebug($"{nameof(HandleDeviceRegistration)} - with Request {request}");

            DeviceReportingStatus status = store.UpdateRegistration(args.DeviceId, request.ReceivedAt);
            var dto = ConvertToStatusDto(status);
            await monitorHub.Clients.All.ReceiveDeviceRegistrationUpdate(dto);
        }


        private async Task HandleDeviceHeartbeat(Request request)
        {
            var args = request.ReceivedData as DevToSrvDeviceHeartbeatDto;

            if (args is null)
            {
                logger.LogError($"{nameof(HandleDeviceHeartbeat)} - Request is not {nameof(DevToSrvDeviceHeartbeatDto)}: {request}");
                return;
            }

            logger.LogDebug($"{nameof(HandleDeviceHeartbeat)} - with Request {request}");

            DeviceReportingStatus status = store.UpdateHeartbeat(args.DeviceId, request.ReceivedAt);
            var dto = ConvertToHeartbeatDto(status);
            await monitorHub.Clients.All.ReceiveDeviceHeartbeatUpdate(dto);
        }

        private async Task HandleMonitorRegistration(Request request)
        {
            var args = request.ReceivedData as MonToSrvRegistrationDto;

            if (args is null)
            {
                logger.LogError($"{nameof(HandleMonitorRegistration)} - Request is not {nameof(MonToSrvRegistrationDto)}: {request}");
                return;
            }


            logger.LogDebug($"{nameof(HandleMonitorRegistration)} - with Request {request}");

            var dto = store.GetDeviceStatuses()
                .Select(status => ConvertToStatusDto(status))
                .ToArray();

            await monitorHub.Clients.Client(request.ConnectionId).ReceiveDeviceStatusesReport(dto);
        }

        private readonly Func<DeviceReportingStatus, SrvToMonDeviceHeartbeatDto> ConvertToHeartbeatDto =
            (status) => new SrvToMonDeviceHeartbeatDto(
                DeviceId: status.DeviceId,
                ReceivedAt: status.LastSeenAt);

        private readonly Func<DeviceReportingStatus, SrvToMonDeviceStatusDto> ConvertToStatusDto =
            (status) => new SrvToMonDeviceStatusDto(
                DeviceId: status.DeviceId,
                FirstRegisteredAt: status.FirstRegisteredAt,
                LastRegisteredAt: status.LastRegisteredAt,
                LastSeenAt: status.LastSeenAt);
    }
}
