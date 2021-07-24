using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTAS.Server.DevicesStatusStore
{
    public class DeviceStatus
    {
        /// <summary>
        /// The human name for the Device
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// // The very first registration DateTime
        /// </summary>
        public DateTime FirstRegisteredAt { get; private set; }

        /// <summary>
        /// The most recent registration <see cref="DateTime"/>
        /// </summary>
        public DateTime LastRegisteredAt { get; private set; }

        /// <summary>
        /// The most recently seen registration or heartbeat <see cref="DateTime"/>
        /// </summary>
        public DateTime LastSeenAt { get; private set; }

    }
}
