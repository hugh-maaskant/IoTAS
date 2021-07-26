using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

using IoTAS.Shared.Hubs;
using IoTAS.Server.Hubs;
using IoTAS.Server.DevicesStatusStore;


namespace IoTAS.Server.InputQueue
{
    public sealed class InputDispatcherHostedService : BackgroundService
    {
        private readonly ILogger<InputDispatcherHostedService> logger;

        private readonly IHubsInputQueueService inputQueue;

        private readonly IDeviceStatusStore store;

        private readonly IHubContext<MonitorHub, IMonitorHub> monitorHub;

        public InputDispatcherHostedService(
            ILogger<InputDispatcherHostedService> logger,
            IHubsInputQueueService inputQueue,
            IDeviceStatusStore store,
            IHubContext<MonitorHub, IMonitorHub> monitorHub)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.monitorHub = monitorHub ?? throw new ArgumentNullException(nameof(monitorHub));

            logger.LogDebug("Created");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting async execution");

            bool fatalError = false;

            while (!stoppingToken.IsCancellationRequested & !fatalError)
            {
                try
                {
                    logger.LogDebug("Waiting for request ...");
                    Request request = await inputQueue.DequeueAsync(stoppingToken);
                    logger.LogDebug($"Retrieved request {request}");

                    await DispatchRequest(request);
                }
                catch (OperationCanceledException e)
                {
                    logger.LogWarning("Cancellation requested - exiting", e);
                }
                catch (Exception e)
                {
                    logger.LogError("Error dequeueng request - exiting", e);
                    fatalError = true; ;
                }
            }

            logger.LogWarning($"Execution stopped fatalError = {fatalError}, cancellation rquested = {stoppingToken.IsCancellationRequested}");
        }

        private async Task DispatchRequest(Request request)
        {
            if (request is null)
            {
                logger.LogWarning($"{nameof(DispatchRequest)} called with request == null");
                return;
            }

            logger.LogInformation($"Dispatching {request}");

            switch (request.ReceivedData)
            {
                case DevToSrvDeviceRegistrationDto:
                    await HandleDeviceRegistration(request);
                    break;
                case DevToSrvDeviceHeartbeatDto:
                    await HandleDeviceHeartbeat(request);
                    break;
                case MonToSrvRegistrationDto:
                    await HandleMonitorRegistration(request);
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

            logger.LogDebug($"{nameof(HandleDeviceRegistration)} handling {request} ...");

            DeviceReportingStatus status = store.UpdateRegistration(args.DeviceId, request.ReceivedAt);
            var dto = ConvertToStatusDto(status);
            await monitorHub.Clients.All.ReceiveDeviceRegistrationUpdate(dto);

            logger.LogDebug($"{nameof(HandleDeviceRegistration)} handling {request} done");

        }


        private async Task HandleDeviceHeartbeat(Request request)
        {
            var args = request.ReceivedData as DevToSrvDeviceHeartbeatDto;

            if (args is null)
            {
                logger.LogError($"{nameof(HandleDeviceHeartbeat)} - Request is not {nameof(DevToSrvDeviceHeartbeatDto)}: {request}");
                return;
            }

            logger.LogDebug($"{nameof(HandleDeviceHeartbeat)} handling {request} ...");

            DeviceReportingStatus status = store.UpdateHeartbeat(args.DeviceId, request.ReceivedAt);
            var dto = ConvertToHeartbeatDto(status);
            await monitorHub.Clients.All.ReceiveDeviceHeartbeatUpdate(dto);

            logger.LogDebug($"{nameof(HandleDeviceHeartbeat)} handling {request} done");

        }

        private async Task HandleMonitorRegistration(Request request)
        {
            var args = request.ReceivedData as MonToSrvRegistrationDto;

            if (args is null)
            {
                logger.LogError($"{nameof(HandleMonitorRegistration)} - Request is not {nameof(MonToSrvRegistrationDto)}: {request}");
                return;
            }


            logger.LogDebug($"{nameof(HandleMonitorRegistration)} handling {request} ...");

            var dto = store.GetDeviceStatuses()
                .Select(status => ConvertToStatusDto(status))
                .ToArray();

            await monitorHub.Clients.Client(request.ConnectionId).ReceiveDeviceStatusesReport(dto);

            logger.LogDebug($"{nameof(HandleMonitorRegistration)} handling {request} done");
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
