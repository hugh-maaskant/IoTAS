//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Text;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Obeserved Device Registration and Heartbeat timing data
    /// </summary>
    /// <remarks>
    /// Send as multicast to all Monitors when the Server receives a Device Registration
    /// and as a singlecast List with all known Devices to the registering Monitor
    /// </remarks>
    public sealed record SrvToMonDeviceStatusDto
    (
        int DeviceId,               // The Id of the Device
        DateTime FirstRegisteredAt, // The very first registration DateTime
        DateTime LastRegisteredAt,  // The most recent registration DateTime
        DateTime LastSeenAt         // The most recently seen registration or heartbeat DateTime 
    )
    {
        private static readonly string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append(nameof(SrvToMonDeviceStatusDto));
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
