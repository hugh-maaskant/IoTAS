using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace IoTAS.Shared.DevicesStatusStore
{
    /// <summary>
    /// Volatile (in memmory) implementation for IDeviceStatusStore
    /// </summary>
    public class VolatileDeviceStatusStore : IDeviceStatusStore
    {
        private readonly ILogger<VolatileDeviceStatusStore> logger;

        private readonly Dictionary<int, DeviceReportingStatus> store = new();

        public VolatileDeviceStatusStore(ILogger<VolatileDeviceStatusStore> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("Store created");
        }

        public int Count { get => store.Count; }

        public void Clear() => store.Clear();

        public DeviceReportingStatus GetDeviceStatus(int deviceId)
        {
            logger.LogDebug(
                nameof(GetDeviceStatus) + " - " +
                "Getting " + 
                nameof(DeviceReportingStatus) + 
                " for Device {DeviceId}",
                deviceId);

            if (store.GetValueOrDefault(deviceId) is DeviceReportingStatus status)
            {
                return status;
            }

            logger.LogWarning(
                nameof(GetDeviceStatus) + " - " +
                "Attempt to retrieve a non-existing " + 
                nameof(DeviceReportingStatus) +
                " for Device {DeviceId}",
                deviceId);

            return new(deviceId, default, default, default);
        }

        public IEnumerable<DeviceReportingStatus> GetDevicesStatusList()
        {
            var values = store.Values;

            logger.LogDebug(
                nameof(GetDevicesStatusList) + " - " +
                "Getting Reporting Status for all {DevicesCount} Devices",
                values.Count);
            
            DeviceReportingStatus[] valuesCopy = new DeviceReportingStatus[values.Count];
            values.CopyTo(valuesCopy, 0);
            
            return valuesCopy;
        }

        public void SetDeviceStatus(DeviceReportingStatus status)
        {
            logger.LogDebug(
                nameof(SetDeviceStatus) + " - " +
                "Insert or update the DeviceReportingStatus for Device {DeviceId}",
                status.DeviceId);

            store[status.DeviceId] =  status;   // overwrites if key exists, otherwise inserts
        }

        public DeviceReportingStatus UpdateHeartbeat(int deviceId, DateTime receivedAt)
        {
            logger.LogDebug(
                nameof(UpdateHeartbeat) + " - " +
                "Heartbeat at {ReceivedAt} for Device {DeviceId}",
                receivedAt, deviceId);

            DeviceReportingStatus current = 
                store.ContainsKey(deviceId) 
                ? GetDeviceStatus(deviceId) 
                : new(deviceId, default, default, default);

            DeviceReportingStatus updated = current with 
            { 
                LastSeenAt = receivedAt 
            };
            
            store[deviceId] = updated;

            return updated;
        }

        public DeviceReportingStatus UpdateRegistration(int deviceId, DateTime receivedAt)
        {
            logger.LogDebug(
                nameof(UpdateRegistration) + " - " +
                "Registration at {ReceivedAt} for Device {DeviceId}",
                receivedAt, deviceId);

            DeviceReportingStatus current =
                store.ContainsKey(deviceId)
                ? GetDeviceStatus(deviceId)  
                : new(deviceId, receivedAt, default, default);

            DeviceReportingStatus updated = current with
            {
                LastRegisteredAt = receivedAt,
                LastSeenAt = receivedAt
            };

            store[deviceId] = updated;

            return updated;
        }        
    }
}
