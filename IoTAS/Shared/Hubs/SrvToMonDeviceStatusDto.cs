using System;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Obeserved Device Registration and Heartbeat timing data
    /// </summary>
    /// <remarks>
    /// Send as multicast to all Monitors when the Server receives a Device Registration
    /// and as a singlecast List with all known Devices to the registering Monitor
    /// </remarks>
    public record SrvToMonDeviceStatusDto
    (
        int DeviceId,          // The Id of the Device
        DateTime FirstRegisteredAt, // The very first registration DateTime
        DateTime LastRegisteredAt,  // The most recent registration DateTime
        DateTime LastSeenAt         // The most recently seen registration or heartbeat DateTime
    );
}
