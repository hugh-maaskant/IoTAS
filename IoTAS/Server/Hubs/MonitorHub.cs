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

/// <summary>
/// Typesafe SignalR Hub for Monitor Clients
/// </summary>
public class MonitorHub : Hub<IMonitorHub>, IMonitorHubServer
{
    private readonly ILogger _logger;

    private readonly IHubsInputQueueService _queueService;

    public MonitorHub(IHubsInputQueueService queueService)
    {
        _logger = Log.ForContext<MonitorHub>();
        _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.Information(
            nameof(OnConnectedAsync) + " - " +
            "Monitor connected on ConnectionId {ConnectionId}",
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception e)
    {
        if (e == null)
        {
            // It's OK for Monitors to go away from their web-page
            _logger.Information(
                nameof(OnDisconnectedAsync) + " - " +
                "Monitor on ConnectionId {ConnectionId} disconnected", 
                Context.ConnectionId);
        }
        else
        {
            _logger.Error(
                e,
                nameof(OnDisconnectedAsync) + " - " +
                "Monitor on ConnectionId {ConnectionId} disconnected with exception", 
                Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(e);
    }

    //
    // Called by the Hub on behalf of the Monitor Client call ...
    //
    public Task RegisterMonitorClient(MonToSrvRegistrationDto monitorRegistrationDto)
    {
        _logger.Information(
            nameof(RegisterMonitorClient) + " - " +
            "Monitor registration received on ConnectionId {ConnectionId}",
            Context.ConnectionId);

        Request request = Request.FromClientCall(Context.ConnectionId, monitorRegistrationDto);

        _queueService.Enqueue(request);

        return Task.CompletedTask;
    }
}