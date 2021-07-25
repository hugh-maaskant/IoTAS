using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace IoTAS.Server.DevicesStatusStore
{
    public class VolatileDeviceStatusStore : IDeviceStatusStore
    {
        private readonly ILogger<VolatileDeviceStatusStore> logger;

        private readonly Dictionary<int, DeviceReportingStatus> store = new();

        public VolatileDeviceStatusStore(ILogger<VolatileDeviceStatusStore> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("Store created");
        }

        DeviceReportingStatus IDeviceStatusStore.GetDeviceStatus(int deviceId)
        {
            logger.LogDebug($"Getting DeviceReportingStatus for Device with DeviceId {deviceId}");
            return store.GetValueOrDefault(deviceId);
        }

        IEnumerable<DeviceReportingStatus> IDeviceStatusStore.GetDeviceStatuses()
        {
            var values = store.Values;
            logger.LogDebug($"Getting DeviceReportingStatus for all {values.Count} Devices");
            
            DeviceReportingStatus[] valuesCopy = new DeviceReportingStatus[values.Count];
            values.CopyTo(valuesCopy, 0);
            return valuesCopy;
        }

        DeviceReportingStatus IDeviceStatusStore.UpateHeartbeat(int deviceId, DateTime receivedAt)
        {
            logger.LogDebug($"Update Heartbeat to {receivedAt} for Device with DeviceId {deviceId}");

            DeviceReportingStatus current = store.GetValueOrDefault(deviceId);
            DeviceReportingStatus updated = current with { LastSeenAt = receivedAt };
            store[deviceId] = updated;

            return updated;
        }

        DeviceReportingStatus IDeviceStatusStore.UpdateRegistration(int deviceId, DateTime receivedAt)
        {
            logger.LogDebug($"Update Registration to {receivedAt} for Device with DeviceId {deviceId}");

            DeviceReportingStatus current = store.GetValueOrDefault(deviceId);
            DateTime firstRegisterdAt = (current.FirstRegisteredAt != DateTime.MinValue) ? current.FirstRegisteredAt : receivedAt;

            DeviceReportingStatus updated = new(
                DeviceId: deviceId,
                FirstRegisteredAt: firstRegisterdAt, 
                LastRegisteredAt: receivedAt, 
                LastSeenAt: receivedAt
                );
            store[deviceId] = updated;

            return updated;
        }

        
    }
}
