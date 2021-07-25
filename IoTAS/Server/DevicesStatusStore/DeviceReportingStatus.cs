using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTAS.Server.DevicesStatusStore
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
    );
}
