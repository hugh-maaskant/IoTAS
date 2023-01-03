//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Threading.Tasks;
using IoTAS.Server.InputQueue;
using IoTAS.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

using Serilog;

namespace IoTAS.Server.Hubs;

public class DeviceHub : Hub<IDeviceHub>, IDeviceHubServer
{
    private readonly ILogger _logger;

    private readonly IHubsInputQueueService _queueService;

    public DeviceHub(IHubsInputQueueService queueService)
    {
        _logger = Log.ForContext<DeviceHub>();
        _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.Information(
            nameof(OnConnectedAsync) + " - " +
            "Device connected on ConnectionId {ConnectionId}", 
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (exception == null)
        {
            _logger.Warning(
                nameof(OnDisconnectedAsync) +" - " +
                "Device on ConnectionId {ConnectionId} disconnected", 
                Context.ConnectionId);
        }
        else
        {
            _logger.Error(
                exception,
                nameof(OnDisconnectedAsync) + " - " + 
                "Device on ConnectionId {ConnectionId} disconnected", 
                Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public Task RegisterDeviceClient(DevToSrvDeviceRegistrationDto dtoIn)
    {
        _logger.Information(
            nameof(RegisterDeviceClient) + " - " +
            "Device registration received from Device {DeviceId} on ConnectionId {ConnectionId}",
            dtoIn.DeviceId, Context.ConnectionId);

        Request request = Request.FromClientCall(Context.ConnectionId, dtoIn);

        _queueService.Enqueue(request);

        return Task.CompletedTask;
    }

    public Task ReceiveDeviceHeartbeat(DevToSrvDeviceHeartbeatDto dtoIn)
    {
        _logger.Information(
            nameof(ReceiveDeviceHeartbeat) + " - " +
            "Heartbeat received from DeviceId {DeviceId} on ConnectionId {ConnectionId}", 
            dtoIn.DeviceId, Context.ConnectionId);

        Request request = Request.FromClientCall(Context.ConnectionId, dtoIn);

        _queueService.Enqueue(request);

        return Task.CompletedTask;
    }        
}