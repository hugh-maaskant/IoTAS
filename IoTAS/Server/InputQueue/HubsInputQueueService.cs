//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

namespace IoTAS.Server.InputQueue;

/// <summary>
/// Queue for messages received on the Hubs
/// </summary>
/// <remarks>
/// Provides a thread-safe Enqueue operation and an asynchronous blocking Dequeue operation
/// </remarks>
public sealed class HubsInputQueueService : IHubsInputQueueService
{
    private readonly object _lockObj = new();

    private readonly SemaphoreSlim _proceedSem = new(0);

    private readonly Queue<Request> _requestsQueue = new();

    private int _maxItemsQueued;
    private int _maxItemsLogged;

    private readonly ILogger _logger;

    public HubsInputQueueService()
    {
        _logger = Log.ForContext<HubsInputQueueService>();

        _logger.Information("Created");
    }

    /// <summary>
    /// Thread-safely Enqueue a Request and allow dequeuing
    /// </summary>
    /// <param name="request">The Request to enqueue</param>
    public void Enqueue(Request request)
    {
        _logger.Debug(
            nameof(Enqueue) + " - " +
            "Dequeuing {Request}", 
            request);

        // needed to avoid race condition in reporting ...
        int maxItemsToLog;

        lock (_lockObj)
        {
            _requestsQueue.Enqueue(request);

            _maxItemsQueued = Math.Max(_maxItemsQueued, _requestsQueue.Count);
            maxItemsToLog = _maxItemsQueued;
        }

        _proceedSem.Release();

        if (maxItemsToLog > _maxItemsLogged)
        {
            _logger.Information(
                nameof(Enqueue) + " - " +
                nameof(_maxItemsQueued) +
                " reached: {MaxItemsQueued}",
                maxItemsToLog);

            _maxItemsLogged = maxItemsToLog;
        }
    }

    /// <summary>
    /// Wait for a Request in the queue while respecting the CancellationToken, threadsafely Dequeue and return it.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>The dequeued Request or <see langword="null"/> in case of cancellation</returns>
    public async Task<Request> DequeueAsync(CancellationToken token)
    {
        try
        {
            await _proceedSem.WaitAsync(token);

            _logger.Debug(
                nameof(DequeueAsync) + " - " +
                "Dequeueing");

            lock (_lockObj)
            {
                Request request = _requestsQueue.Dequeue();
                return request;
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Error(
                e, 
                nameof(DequeueAsync) + " - " + "Wait operation cancelled");
            throw;
        }
        catch (Exception e)
        {
            _logger.Error(
                e, 
                nameof(DequeueAsync) + " - " + "Wait operation exception");
            throw;
        }
    }

    public void Dispose()
    {
        _proceedSem?.Dispose();

        _logger.Information(
            nameof(Dispose) + " - " + "Disposed ...");
    }
}