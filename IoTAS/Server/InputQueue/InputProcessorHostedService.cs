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
    /// <summary>
    /// Gets Requests from the HubsInputQueue, stores the Device's Registration and 
    /// Heartbeat data in the InputQueue, and dispatches any actions through the Hubs
    /// </summary>
    public sealed class InputProcessorHostedService : BackgroundService
    {
        private readonly ILogger<InputProcessorHostedService> logger;

        private readonly IHubsInputQueue inputQueue;

        private readonly IDeviceStatusStore store;

        private readonly IHubContext<MonitorHub, IMonitorHub> monitorHubContext;

        private readonly MonitorHub monitorHub;

        public InputProcessorHostedService(
            ILogger<InputProcessorHostedService> logger,
            IHubsInputQueue inputQueue,
            IDeviceStatusStore store,
            IHubContext<MonitorHub, IMonitorHub> monitorHubContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.monitorHubContext = monitorHubContext ?? throw new ArgumentNullException(nameof(monitorHubContext));
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

                    await ProcessRequest(request);
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

        private async Task ProcessRequest(Request request)
        {
            if (request is null)
            {
                logger.LogWarning(
                    nameof(ProcessRequest) + " - " +
                    "Called with request == null");
                
                return;
            }

            logger.LogInformation(
                nameof(ProcessRequest) + " - " + 
                "Dispatching {Request}",
                request);

            switch (request.ReceivedDto)
            {
                case DevToSrvDeviceRegistrationDto:
                    await ProcessDeviceRegistration(request);
                    break;

                case DevToSrvDeviceHeartbeatDto:
                    await ProcessDeviceHeartbeat(request);
                    break;

                case MonToSrvRegistrationDto:
                    await ProcessMonitorRegistration(request);
                    break;

                default:
                    logger.LogError(
                        nameof(ProcessRequest) + " - " +
                        "Unknown request.ReceivedDto type : {Request}",
                        request);
                    break;
            }
        }

        private async Task ProcessDeviceRegistration(Request request)
        {
            Debug.Assert(
                request.ReceivedDto is DevToSrvDeviceRegistrationDto,
                $"{nameof(ProcessDeviceRegistration)} request.ReceivedData is not {nameof(DevToSrvDeviceRegistrationDto)}");

            var dtoIn = (DevToSrvDeviceRegistrationDto)request.ReceivedDto;

            logger.LogDebug(
                nameof(ProcessDeviceRegistration) + " - " +
                "Handling {Request}",
                request);

            DeviceReportingStatus status = store.UpdateRegistration(dtoIn.DeviceId, request.ReceivedAt);
            var dtoOut = ConvertToStatusDto(status);
            await monitorHubContext.Clients.All.ReceiveDeviceStatusUpdate(dtoOut);

            logger.LogDebug(
                nameof(ProcessDeviceRegistration) + " - " +
                "Handled {Request}",
                request);
        }

        private async Task ProcessDeviceHeartbeat(Request request)
        {
            Debug.Assert(
                request.ReceivedDto is DevToSrvDeviceHeartbeatDto,
                $"{nameof(ProcessDeviceHeartbeat)} request.ReceivedData is not {nameof(DevToSrvDeviceHeartbeatDto)}");

            var dtoIn = (DevToSrvDeviceHeartbeatDto)request.ReceivedDto;

            logger.LogDebug(
                nameof(ProcessDeviceHeartbeat) + " - " +
                "Handling {Request}",
                request);

            DeviceReportingStatus status = store.UpdateHeartbeat(dtoIn.DeviceId, request.ReceivedAt);
            var dtoOut = ConvertToHeartbeatDto(status);
            await monitorHubContext.Clients.All.ReceiveDeviceHeartbeatUpdate(dtoOut);

            logger.LogDebug(
                nameof(ProcessDeviceHeartbeat) + " - " +
                "Handled {Request}",
                request);
        }

        private async Task ProcessMonitorRegistration(Request request)
        {
            Debug.Assert(
                request.ReceivedDto is MonToSrvRegistrationDto,
                $"{nameof(ProcessMonitorRegistration)} request.ReceivedData is not {nameof(MonToSrvRegistrationDto)}");

            var dtoIn = (MonToSrvRegistrationDto)request.ReceivedDto;

            logger.LogDebug(
                nameof(ProcessMonitorRegistration) + " - " +
                "Handling {Request}",
                request);

            var dtoOut = store.GetDeviceStatuses()
                .Select(status => ConvertToStatusDto(status))
                .ToArray();

            await monitorHubContext.Clients.Client(request.ConnectionId).ReceiveDeviceStatusesSnapshot(dtoOut);

            logger.LogDebug(
                nameof(ProcessMonitorRegistration) + " - " +
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
