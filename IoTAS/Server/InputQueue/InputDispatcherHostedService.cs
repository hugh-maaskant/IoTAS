using System;
using System.Diagnostics;
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
            logger.LogInformation(
                nameof(ExecuteAsync) + " - " +
                "Starting Async execution");

            bool fatalError = false;

            while (!stoppingToken.IsCancellationRequested & !fatalError)
            {
                try
                {
                    logger.LogDebug(
                        nameof(ExecuteAsync) + " - " +
                        "Waiting for request ...");
                    
                    Request request = await inputQueue.DequeueAsync(stoppingToken);
                    
                    logger.LogDebug(
                        nameof(ExecuteAsync) + " - " +
                        "Retrieved {Request}",
                        request);

                    await DispatchRequest(request);
                }
                catch (OperationCanceledException e)
                {
                    logger.LogWarning(
                        e,
                        nameof(ExecuteAsync) + " - " +
                        "Cancellation requested - EXITING");
                }
                catch (Exception e)
                {
                    logger.LogError(
                        e,
                        nameof(ExecuteAsync) + " - " + 
                        "Error dequeueng request - EXITING");
                    fatalError = true; ;
                }
            }

            logger.LogWarning(
                nameof(ExecuteAsync) + " - " +
                $"Execution stopped fatalError = {fatalError}, cancellation rquested = {stoppingToken.IsCancellationRequested}");
        }

        private async Task DispatchRequest(Request request)
        {
            if (request is null)
            {
                logger.LogWarning(
                    nameof(DispatchRequest) + " - " +
                    "Called with request == null");
                
                return;
            }

            logger.LogInformation(
                nameof(DispatchRequest) + " - " + 
                "Dispatching {Request}",
                request);

            switch (request.ReceivedDto)
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
                    logger.LogError(
                        nameof(DispatchRequest) + " - " +
                        "Unknown request.ReceivedDto type : {Request}",
                        request);
                    break;
            }
        }

        private async Task HandleDeviceRegistration(Request request)
        {
            Debug.Assert(
                request.ReceivedDto is DevToSrvDeviceRegistrationDto,
                $"{nameof(HandleDeviceRegistration)} request.ReceivedData is not {nameof(DevToSrvDeviceRegistrationDto)}");

            var dtoIn = (DevToSrvDeviceRegistrationDto)request.ReceivedDto;

            logger.LogDebug(
                nameof(HandleDeviceRegistration) + " - " +
                "Handling {Request}",
                request);

            DeviceReportingStatus status = store.UpdateRegistration(dtoIn.DeviceId, request.ReceivedAt);
            var dtoOut = ConvertToStatusDto(status);
            await monitorHub.Clients.All.ReceiveDeviceRegistrationUpdate(dtoOut);

            logger.LogDebug(
                nameof(HandleDeviceRegistration) + " - " +
                "Handled {Request}",
                request);
        }

        private async Task HandleDeviceHeartbeat(Request request)
        {
            Debug.Assert(
                request.ReceivedDto is DevToSrvDeviceHeartbeatDto,
                $"{nameof(HandleDeviceHeartbeat)} request.ReceivedData is not {nameof(DevToSrvDeviceHeartbeatDto)}");

            var dtoIn = (DevToSrvDeviceHeartbeatDto)request.ReceivedDto;

            logger.LogDebug(
                nameof(HandleDeviceHeartbeat) + " - " +
                "Handling {Request}",
                request);

            DeviceReportingStatus status = store.UpdateHeartbeat(dtoIn.DeviceId, request.ReceivedAt);
            var dtoOut = ConvertToHeartbeatDto(status);
            await monitorHub.Clients.All.ReceiveDeviceHeartbeatUpdate(dtoOut);

            logger.LogDebug(
                nameof(HandleDeviceHeartbeat) + " - " +
                "Handled {Request}",
                request);
        }

        private async Task HandleMonitorRegistration(Request request)
        {
            Debug.Assert(
                request.ReceivedDto is MonToSrvRegistrationDto,
                $"{nameof(HandleMonitorRegistration)} request.ReceivedData is not {nameof(MonToSrvRegistrationDto)}");

            var dtoIn = (MonToSrvRegistrationDto)request.ReceivedDto;

            logger.LogDebug(
                nameof(HandleMonitorRegistration) + " - " +
                "Handling {Request}",
                request);

            var dtoOut = store.GetDeviceStatuses()
                .Select(status => ConvertToStatusDto(status))
                .ToArray();

            await monitorHub.Clients.Client(request.ConnectionId).ReceiveDeviceStatusesReport(dtoOut);

            logger.LogDebug(
                nameof(HandleMonitorRegistration) + " - " +
                "Handled {Request}",
                request);
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
