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
    internal record DeviceReportingStatus
    (
        /// <summary>
        /// // The date and time of the very first registration
        /// </summary>
        DateTime FirstRegisteredAt,

        /// <summary>
        /// The date and time of most recent registration
        /// </summary>
        DateTime LastRegisteredAt,

        /// <summary>
        /// The date and time of most recently seen registration or heartbeat
        /// </summary>
        DateTime LastSeenAt
    );
}
