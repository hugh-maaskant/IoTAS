using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using IoTAS.Shared.DevicesStatusStore;
using IoTAS.Shared.Hubs;
using IoTAS.Server.Hubs;


namespace IoTAS.Server.InputQueue
{
    /// <summary>
    /// Gets Requests from the HubsInputQueue, stores the Device's Registration and 
    /// Heartbeat data in the InputQueue, and dispatches any actions through the Hubs
    /// </summary>
    public sealed class InputProcessorHostedService : BackgroundService
    {
        private const string registeredMonitorsGroup = "RegisteredMonitors";

        private readonly ILogger<InputProcessorHostedService> logger;

        private readonly IHubsInputQueueService inputQueue;

        private readonly IDeviceStatusStore store;

        private readonly IHubContext<MonitorHub, IMonitorHub> monitorHubContext;

        public InputProcessorHostedService(
            ILogger<InputProcessorHostedService> logger,
            IHubsInputQueueService inputQueue,
            IDeviceStatusStore store,
            IHubContext<MonitorHub, IMonitorHub> monitorHubContext)
        {
            this.logger = logger ?? NullLogger<InputProcessorHostedService>.Instance;
            this.inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.monitorHubContext = monitorHubContext ?? throw new ArgumentNullException(nameof(monitorHubContext));
            
            logger.LogDebug("Created");
        }

        // Called by the ASP.NET infrastructure upon startup ...
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation(
                nameof(ExecuteAsync) + " - " +
                "Starting Async execution");

            bool fatalError = false;

            while (!stoppingToken.IsCancellationRequested && !fatalError)
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
                    await ProcessDeviceRegistrationAsync(
                        (DevToSrvDeviceRegistrationDto)request.ReceivedDto, request.ReceivedAt);
                    break;

                case DevToSrvDeviceHeartbeatDto:
                    await ProcessDeviceHeartbeatASync(
                        (DevToSrvDeviceHeartbeatDto)request.ReceivedDto, request.ReceivedAt);
                    break;

                case MonToSrvRegistrationDto:
                    await ProcessMonitorRegistrationAsync(
                        (MonToSrvRegistrationDto)request.ReceivedDto, request.ConnectionId);
                    break;

                default:
                    logger.LogError(
                        nameof(ProcessRequest) + " - " +
                        "Unknown request.ReceivedDto type : {Request}",
                        request);
                    break;
            }
        }

        private async Task ProcessDeviceRegistrationAsync(DevToSrvDeviceRegistrationDto dtoIn, DateTime timeStamp)
        {
            DeviceReportingStatus status = store.UpdateRegistration(dtoIn.DeviceId, timeStamp);

            var dtoOut = status.ToStatusDto();

            try
            {
                await monitorHubContext.Clients.Group(registeredMonitorsGroup)
                    .ReceiveDeviceStatusUpdate(dtoOut);
            }
            catch (Exception e)
            {
                logger.LogWarning(
                    e,
                    nameof(ProcessDeviceRegistrationAsync) + " - " +
                    $"Error in {nameof(IMonitorHub.ReceiveDeviceStatusUpdate)} multicast");
            }

            logger.LogDebug(
                nameof(ProcessDeviceRegistrationAsync) + " - " +
                "Handled {dtoIn}",
                dtoIn);
        }

        private async Task ProcessDeviceHeartbeatASync(DevToSrvDeviceHeartbeatDto dtoIn, DateTime timeStamp)
        {
            DeviceReportingStatus status = store.UpdateHeartbeat(dtoIn.DeviceId, timeStamp);

            var dtoOut = status.ToHeartbeatDto();

            try
            {
                await monitorHubContext.Clients.Group(registeredMonitorsGroup)
                    .ReceiveDeviceHeartbeatUpdate(dtoOut);
            }
            catch (Exception e)
            {
                logger.LogWarning(
                    e,
                    nameof(ProcessDeviceHeartbeatASync) +
                    $"Error in {nameof(IMonitorHub.ReceiveDeviceHeartbeatUpdate)} multicast");
            }
            logger.LogDebug(
                nameof(ProcessDeviceHeartbeatASync) + " - " +
                "Handled {dtoIn}",
                dtoIn);
        }

        private async Task ProcessMonitorRegistrationAsync(MonToSrvRegistrationDto dtoIn, string connectionId)
        {
            var dtoOut = store.GetDevicesStatusList()
                .Select(status => status.ToStatusDto())
                .ToArray();

            try
            {
                // Provide the registering Monitor with the current status of all Devices
                // If the number of devices gets too big, this could conditially be done in chunks
                await monitorHubContext.Clients.Client(connectionId)
                    .ReceiveDeviceStatusesSnapshot(dtoOut);
            }
            catch (Exception e)
            {
                logger.LogWarning(
                    e,
                    nameof(ProcessMonitorRegistrationAsync) +
                    $"Error in {nameof(IMonitorHub.ReceiveDeviceStatusesSnapshot)} singlecast");
            }

            // Ensure it will get future status updates
            await monitorHubContext.Groups.AddToGroupAsync(connectionId, registeredMonitorsGroup);

            logger.LogDebug(
                nameof(ProcessMonitorRegistrationAsync) + " - " +
                "Handled {dtoIn}",
                dtoIn);
        }
    }
}
