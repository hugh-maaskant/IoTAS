﻿

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Monitor Attributes that are passed to the Server during Registration.
    /// </summary>
    /// <remarks>
    /// This struct provides options to extend the 
    /// IMonitorHubServer.RegisterMonitor() operation with new data elements
    /// without being a breaking change on the interface itself (as long as
    /// the server can deal with any of its historic variants. 
    /// </remarks>
    public record MonToSrvRegistrationDto : BaseHubInDto
    {
        // empty at this time
    }
}