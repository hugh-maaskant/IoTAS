using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IoTAS.Shared.Hubs;

namespace IoTAS.Shared.DevicesStatusStore
{
    /// <summary>
    /// Records the currently observed Device status, which is based on registrations 
    /// and heartbeat DeviceHub input received
    /// </summary>
    public record DeviceReportingStatus
    (
        // Also the key to the record
        int DeviceId,

        // The date and time of the very first registration
        DateTime FirstRegisteredAt,

        // The date and time of most recent registration
        DateTime LastRegisteredAt,

        // The date and time of most recently seen registration or heartbeat
        DateTime LastSeenAt
    )
    {
        public SrvToMonDeviceStatusDto ToStatusDto()
        {
            return new SrvToMonDeviceStatusDto(
                DeviceId: DeviceId,
                FirstRegisteredAt: FirstRegisteredAt,
                LastRegisteredAt: LastRegisteredAt,
                LastSeenAt: LastSeenAt);
        }

        public SrvToMonDeviceHeartbeatDto ToHeartbeatDto()
        {
            return new SrvToMonDeviceHeartbeatDto(
                DeviceId: DeviceId,
                ReceivedAt: LastSeenAt);
        }

        public static DeviceReportingStatus FromStatusDto(SrvToMonDeviceStatusDto statusDto)
        {
            return new DeviceReportingStatus(
                DeviceId: statusDto.DeviceId,
                FirstRegisteredAt: statusDto.FirstRegisteredAt,
                LastRegisteredAt: statusDto.LastRegisteredAt,
                LastSeenAt: statusDto.LastSeenAt);
        }
    }
}
