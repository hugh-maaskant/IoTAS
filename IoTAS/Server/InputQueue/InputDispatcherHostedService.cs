using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoTAS.Server.InputQueue
{
    public class InputDispatcherHostedService : BackgroundService
    {
        private readonly ILogger<InputDispatcherHostedService> logger;

        private readonly IHubsInputQueueService inputQueue;

        public InputDispatcherHostedService(
            ILogger<InputDispatcherHostedService> logger,
            IHubsInputQueueService inputQueue)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));

            inputQueue.Enqueue(new Request(DateTime.Now, new Shared.Hubs.DevToSrvDeviceHeartbeatArgs(5)));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting async execution");

            bool fatalError = false;

            while (!stoppingToken.IsCancellationRequested & !fatalError)
            {
                try
                {
                    Request request = await inputQueue.DequeueAsync(stoppingToken);

                    logger.LogInformation("Retrieved request {request}");
                }
                catch (OperationCanceledException e)
                {
                    logger.LogWarning("Cancellation requested - exiting", e);
                }
                catch (Exception e)
                {
                    logger.LogError("error dequeueng request - exiting", e);
                    fatalError = true; ;
                }
            }

            logger.LogInformation("Execution stopped");
        }
    }
}
