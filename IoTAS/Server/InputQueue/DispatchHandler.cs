using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using IoTAS.Server.Hubs;
using IoTAS.Shared.Hubs;

namespace IoTAS.Server.InputQueue
{
    public sealed class DispatchHandler
    {
        private readonly ILogger<DispatchHandler> logger;

        // private readonly MonitorHub monitorHub;

        public DispatchHandler(ILogger<DispatchHandler> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Dispatch(Request request)
        {
            if (request is null)
            {
                logger.LogWarning($"Dispatch called with request == null");
                return;
            }

            // just for testing the build
            await Task.Delay(1);

            return;
        }
    }
}
