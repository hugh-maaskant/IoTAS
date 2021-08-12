using System;
using System.Text;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Send to Monitors whne the Server receives a Device Heartbeat
    /// </summary>
    public sealed record SrvToMonDeviceHeartbeatDto
    (
        int DeviceId,          // The Id of the Device
        DateTime ReceivedAt    // The DateTime that the HeartBeat was received
    )

    {
        private static readonly string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append(nameof(SrvToMonDeviceHeartbeatDto));
            sb.Append("{ ");
            sb.Append(nameof(DeviceId));
            sb.Append(" = ");
            sb.Append(DeviceId);
            sb.Append(", ");
            sb.Append(nameof(ReceivedAt));
            sb.Append(" = ");
            sb.Append(ReceivedAt.ToString(dateTimeFormat));
            sb.Append(" }");

            return sb.ToString();
        }
    }
}
