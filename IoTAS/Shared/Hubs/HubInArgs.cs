using System;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Marker interface for Hub input and connection Event records from
    /// Clients (devices and Monitors) to the Server's SignalR Hubs
    /// </summary>
    public abstract record HubInArgs
    {
        // Empty marker interface
    }
}
