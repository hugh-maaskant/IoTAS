using System;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Send to Monitors whne the Server receives a Device Heartbeat
    /// </summary>
    public record SrvToMonDeviceHeartbeatDto
    (
        int DeviceId,          // The Id of the Device
        DateTime ReceivedAt    // The DateTime that the HeartBeat was received
    );
}
