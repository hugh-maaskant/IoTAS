//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IoTAS.Server.Hubs;
using IoTAS.Shared.DevicesStatusStore;
using IoTAS.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace IoTAS.Server.InputQueue;

/// <summary>
/// Gets Requests from the HubsInputQueue, stores the Device's Registration and 
/// Heartbeat data in the InputQueue, and dispatches any actions through the Hubs
/// </summary>
public sealed class InputProcessorHostedService : BackgroundService
{
    private const string RegisteredMonitorsGroup = "RegisteredMonitors";

    private readonly ILogger _logger;

    private readonly IHubsInputQueueService _inputQueue;

    private readonly IDeviceStatusStore _store;

    private readonly IHubContext<MonitorHub, IMonitorHub> _monitorHubContext;

    public InputProcessorHostedService(
        IHubsInputQueueService inputQueue,
        IDeviceStatusStore store,
        IHubContext<MonitorHub, IMonitorHub> monitorHubContext)
    {
        _logger = Log.ForContext<InputProcessorHostedService>();
            
        _inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _monitorHubContext = monitorHubContext ?? throw new ArgumentNullException(nameof(monitorHubContext));
            
        _logger.Debug("Created");
    }

    // Called by the ASP.NET infrastructure upon startup ...
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information(
            nameof(ExecuteAsync) + " - " +
            "Starting Async execution");

        bool fatalError = false;

        while (!stoppingToken.IsCancellationRequested && !fatalError)
        {
            try
            {
                _logger.Debug(
                    nameof(ExecuteAsync) + " - " +
                    "Waiting for request ...");
                    
                Request request = await _inputQueue.DequeueAsync(stoppingToken);
                    
                _logger.Debug(
                    nameof(ExecuteAsync) + " - " +
                    "Retrieved {Request}",
                    request);

                await ProcessRequest(request);
            }
            catch (OperationCanceledException e)
            {
                _logger.Warning(
                    e,
                    nameof(ExecuteAsync) + " - " +
                    "Cancellation requested - EXITING");
            }
            catch (Exception e)
            {
                _logger.Error(
                    e,
                    nameof(ExecuteAsync) + " - " + 
                    "Error dequeuing request - EXITING");
                fatalError = true;
            }
        }

        _logger.Warning(
            nameof(ExecuteAsync) + " - " +
            $"Execution stopped fatalError = {fatalError}, cancellation requested = {stoppingToken.IsCancellationRequested}");
    }

    private async Task ProcessRequest(Request request)
    {
        if (request is null)
        {
            _logger.Warning(
                nameof(ProcessRequest) + " - " +
                "Called with request == null");
                
            return;
        }

        _logger.Information(
            nameof(ProcessRequest) + " - " + 
            "Dispatching {Request}",
            request);

        switch (request.ReceivedDto)
        {
            case DevToSrvDeviceRegistrationDto dto:
                await ProcessDeviceRegistrationAsync(dto, request.ReceivedAt);
                break;

            case DevToSrvDeviceHeartbeatDto dto:
                await ProcessDeviceHeartbeatASync(dto, request.ReceivedAt);
                break;

            case MonToSrvRegistrationDto dto:
                await ProcessMonitorRegistrationAsync(dto, request.ConnectionId);
                break;

            default:
                _logger.Error(
                    nameof(ProcessRequest) + " - " +
                    "Unknown request.ReceivedDto type : {Request}",
                    request);
                break;
        }
    }

    private async Task ProcessDeviceRegistrationAsync(DevToSrvDeviceRegistrationDto dtoIn, DateTime timeStamp)
    {
        DeviceReportingStatus status = _store.UpdateRegistration(dtoIn.DeviceId, timeStamp);

        var dtoOut = status.ToStatusDto();

        try
        {
            await _monitorHubContext.Clients.Group(RegisteredMonitorsGroup)
                .ReceiveDeviceStatusUpdate(dtoOut);
        }
        catch (Exception e)
        {
            _logger.Warning(
                e,
                nameof(ProcessDeviceRegistrationAsync) + " - " +
                $"Error in {nameof(IMonitorHub.ReceiveDeviceStatusUpdate)} multicast");
        }

        _logger.Debug(
            nameof(ProcessDeviceRegistrationAsync) + " - " +
            "Handled {dtoIn}",
            dtoIn);
    }

    private async Task ProcessDeviceHeartbeatASync(DevToSrvDeviceHeartbeatDto dtoIn, DateTime timeStamp)
    {
        DeviceReportingStatus status = _store.UpdateHeartbeat(dtoIn.DeviceId, timeStamp);

        var dtoOut = status.ToHeartbeatDto();

        try
        {
            await _monitorHubContext.Clients.Group(RegisteredMonitorsGroup)
                .ReceiveDeviceHeartbeatUpdate(dtoOut);
        }
        catch (Exception e)
        {
            _logger.Warning(
                e,
                nameof(ProcessDeviceHeartbeatASync) +
                $"Error in {nameof(IMonitorHub.ReceiveDeviceHeartbeatUpdate)} multicast");
        }
        _logger.Debug(
            nameof(ProcessDeviceHeartbeatASync) + " - " +
            "Handled {dtoIn}",
            dtoIn);
    }

    private async Task ProcessMonitorRegistrationAsync(MonToSrvRegistrationDto dtoIn, string connectionId)
    {
        var dtoOut = _store.GetDevicesStatusList()
            .Select(status => status.ToStatusDto())
            .ToArray();

        try
        {
            // Provide the registering Monitor with the current status of all Devices
            // If the number of devices gets too big, this could conditially be done in chunks
            await _monitorHubContext.Clients.Client(connectionId)
                .ReceiveDeviceStatusesSnapshot(dtoOut);
        }
        catch (Exception e)
        {
            _logger.Warning(
                e,
                nameof(ProcessMonitorRegistrationAsync) +
                $"Error in {nameof(IMonitorHub.ReceiveDeviceStatusesSnapshot)} singlecast");
        }

        // Ensure it will get future status updates
        await _monitorHubContext.Groups.AddToGroupAsync(connectionId, RegisteredMonitorsGroup);

        _logger.Debug(
            nameof(ProcessMonitorRegistrationAsync) + " - " +
            "Handled {dtoIn}",
            dtoIn);
    }
}