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
    public class HubsInputQueue
    {
        private readonly object lockObj = new();

        private readonly SemaphoreSlim proceed = new SemaphoreSlim(0);

        private readonly Queue<Request> requests = new();

        private int maxQueuedItems = 0;
        private int maxLoggedItems = 0;

        private readonly ILogger<HubsInputQueue> logger;

        public HubsInputQueue(ILogger<HubsInputQueue> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Threadsafely Enqueue a Request and allow dequeueing
        /// </summary>
        /// <param name="request">The Request to enqueue</param>
        public void Enqueue(Request request)
        {
            logger.LogInformation("Enqueueing");

            lock(lockObj)
            {
                requests.Enqueue(request);
                maxQueuedItems = Math.Max(maxQueuedItems, requests.Count);
            }

            proceed.Release();

            if(maxQueuedItems > maxLoggedItems)
            {
                logger.LogInformation($"MaxQueuedItems reached: {maxQueuedItems}");
            }
        }

        /// <summary>
        /// Blocking wait for a Request in the queue and threadsafely Dequeue it
        /// </summary>
        /// <returns>The dequeued Request</returns>
        public Request Dequeue()
        {
            proceed.Wait();

            logger.LogInformation("Dequeueing");

            Request request;

            lock(lockObj)
            {
                request = requests.Dequeue();
            }

            return request;
        }
    }
}
