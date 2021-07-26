using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IoTAS.Server.InputQueue
{
    /// <summary>
    /// Queue for messages received on the Hubs
    /// </summary>
    /// <remarks>
    /// Provides a thread-safe Enqueue operation and an asynchronous blocking Dequeue operation
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
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.logger.LogInformation("Created");
        }

        /// <summary>
        /// Threadsafely Enqueue a Request and allow dequeueing
        /// </summary>
        /// <param name="request">The Request to enqueue</param>
        public void Enqueue(Request request)
        {
            logger.LogDebug(
                nameof(Enqueue) + " - " +
                "Enqueueing {Request}", 
                request);

            // needed to avoid race condition in reporting ...
            int maxItemsToLog;

            lock (lockObj)
            {
                requestsQueue.Enqueue(request);

                maxItemsQueued = Math.Max(maxItemsQueued, requestsQueue.Count);
                maxItemsToLog = maxItemsQueued;
            }

            proceedSem.Release();

            if (maxItemsToLog > maxItemsLogged)
            {
                logger.LogInformation(
                    nameof(Enqueue) + " - " +
                    nameof(maxItemsQueued) +
                    " reached: {MaxItemsQueued}",
                    maxItemsToLog);

                maxItemsLogged = maxItemsToLog;
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
                await proceedSem.WaitAsync(token);

                logger.LogDebug(
                    nameof(DequeueAsync) + " - " +
                    "Dequeueing");

                lock (lockObj)
                {
                    Request request = requestsQueue.Dequeue();
                    return request;
                }
            }
            catch (OperationCanceledException e)
            {
                logger.LogWarning(
                    nameof(DequeueAsync) + " - " +
                    "Wait operation cancelled", e);
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(
                    e, 
                    nameof(DequeueAsync) + " - " + 
                    "Wait operation exception");
                throw;
            }
        }

        public void Dispose()
        {
            proceedSem?.Dispose();

            logger.LogInformation(
                nameof(Dispose) + " - " +
                "Disposed ...");
        }
    }
}
