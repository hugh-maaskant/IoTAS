using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IoTAS.Shared.Hubs;

namespace IoTAS.Shared.DevicesStatusStore
{
    /// <summary>
    /// Records the currently observed Device status, which is based on registrations 
    /// and heartbeat DeviceHub input received
    /// </summary>
    public sealed record DeviceReportingStatus
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
        private static readonly string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

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

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append(nameof(DeviceReportingStatus));
            sb.Append(" { ");
            sb.Append(nameof(DeviceId));
            sb.Append(" = ");
            sb.Append(DeviceId);
            sb.Append(", ");
            sb.Append(nameof(FirstRegisteredAt));
            sb.Append(" = ");
            sb.Append(FirstRegisteredAt.ToString(dateTimeFormat));
            sb.Append(", ");
            sb.Append(nameof(LastRegisteredAt));
            sb.Append(" = ");
            sb.Append(LastRegisteredAt.ToString(dateTimeFormat));
            sb.Append(", ");
            sb.Append(nameof(LastSeenAt));
            sb.Append(" = ");
            sb.Append(LastSeenAt.ToString(dateTimeFormat));
            sb.Append(" } ");

            return sb.ToString();
        }
    }

}

