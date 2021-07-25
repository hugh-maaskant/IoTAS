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

        public DeviceReportingStatus GetDeviceStatus(int deviceId)
        {
            logger.LogDebug($"Getting DeviceReportingStatus for Device with DeviceId {deviceId}");
            if (store.GetValueOrDefault(deviceId) is DeviceReportingStatus status)
            {
                return status;
            }

            logger.LogWarning($"{nameof(GetDeviceStatus)} attempted to retrieve a non-existing {nameof(DeviceReportingStatus)} for DeviceId {deviceId}");
            return new(deviceId, default, default, default);
        }

        public IEnumerable<DeviceReportingStatus> GetDeviceStatuses()
        {
            var values = store.Values;
            logger.LogDebug($"{nameof(GetDeviceStatuses)} for all {values.Count} Devices");
            
            DeviceReportingStatus[] valuesCopy = new DeviceReportingStatus[values.Count];
            values.CopyTo(valuesCopy, 0);
            return valuesCopy;
        }

        public DeviceReportingStatus UpdateHeartbeat(int deviceId, DateTime receivedAt)
        {
            logger.LogDebug($"{nameof(UpdateHeartbeat)} to {receivedAt} for Device with DeviceId {deviceId}");

            DeviceReportingStatus current = GetDeviceStatus(deviceId);
            DeviceReportingStatus updated = current with { LastSeenAt = receivedAt };
            store[deviceId] = updated;

            return updated;
        }

        public DeviceReportingStatus UpdateRegistration(int deviceId, DateTime receivedAt)
        {
            logger.LogDebug($"{nameof(UpdateRegistration)} to {receivedAt} for DeviceId {deviceId}");

            DeviceReportingStatus current = GetDeviceStatus(deviceId);
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
