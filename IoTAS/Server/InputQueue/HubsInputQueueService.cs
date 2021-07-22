using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace IoTAS.Server.InputQueue
{
    /// <summary>
    /// Queue for messages received on the Hubs
    /// </summary>
    /// <remarks>
    /// Provides thread-safe operations and a blocking Dequeue operation
    /// </remarks>
    public sealed class HubsInputQueueService : IHubsInputQueueService
    {
        private readonly object lockObj = new();

        private readonly SemaphoreSlim proceedSem = new(0);

        private readonly Queue<Request> requestsQueue = new();

        private int maxItemsQueued = 0;
        private int maxItemsLogged = 0;

        private readonly ILogger<HubsInputQueueService> logger;

        public HubsInputQueueService(ILogger<HubsInputQueueService> logger)
        {
            this.logger = logger;
            this.logger.LogInformation("Created");
        }

        /// <summary>
        /// Threadsafely Enqueue a Request and allow dequeueing
        /// </summary>
        /// <param name="request">The Request to enqueue</param>
        public void Enqueue(Request request)
        {
            logger.LogDebug("Enqueueing");

            lock (lockObj)
            {
                requestsQueue.Enqueue(request);
                maxItemsQueued = Math.Max(maxItemsQueued, requestsQueue.Count);
            }

            proceedSem.Release();

            if (maxItemsQueued > maxItemsLogged)
            {
                logger.LogInformation($"MaxQueuedItems reached: {maxItemsQueued}");
                maxItemsLogged = maxItemsQueued;
            }
        }

        /// <summary>
        /// Blocking wait for a Request in the queue 
        /// (respecting the CancellationToken), threadsafely Dequeue and return it.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>The dequeued Request or <see langword="null"/> in case of cancellation</returns>
        public Request? Dequeue(CancellationToken token)
        {
            Request request = null;

            try
            {
                proceedSem.Wait(token);

                logger.LogDebug("Dequeueing");

                lock (lockObj)
                {
                    request = requestsQueue.Dequeue();
                }
            }
            catch (OperationCanceledException e)
            {
                logger.LogWarning($"Dequeue - Wait operation cancelled: {e.Message}");
            }
            catch (Exception e)
            {
                logger.LogError("Dequeue - Wait operation exception", e);
            }

            return request;
        }

        public void Dispose()
        {
            proceedSem?.Dispose();
            logger.LogInformation("Disposed");
        }
    }
}
