﻿  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    public record DeviceHeartbeatOutDTO
    (
        int      DeviceId,          // The Id of the Device
        DateTime ReceivedAt         // The DateTime that the HeartBeat was received
    );

    public record DeviceStatusOutDTO
    (
        int      DeviceId,          // The Id of the Device
        string   DeviceName,        // The human name for the Device
        DateTime FirstRegisteredAt, // The very first registration DateTime
        DateTime LastRegisteredAt,  // The most recent registration DateTime
        DateTime LastSeenAt         // The most recently seen registration or heartbeat DateTime
    );

    /// <summary>
    /// Typesafe Hub Interface for the IoTAS Server to signal (call) the Monitor Clients
    /// </summary>
    public interface IMonitorHubClient
    {
        /// <summary>
        /// A message to notify Monitor(s) of a received Device Heartbeat
        /// </summary>
        /// <param name="deviceId">The DeviceId of the bespoke Device</param>
        /// <param name="receivedAt">The DateTime that the heartbeat was received</param>
        /// <param name="attributes">Aptional heartbead attributes</param>
        /// <returns>A Task</returns>
        public Task DeviceHeartBeatUpdate(DeviceHeartbeatOutDTO hearbeatAttributes);

        /// <summary>
        /// A message to inform a Monitor of the current Devices status
        /// </summary>
        /// <param name="statusAttributesList">A list of Device status attributes</param>
        /// <returns>A Task</returns>
        /// <remarks>
        /// When there are too many Devices, the server may send this message 
        /// multiple times (i.e. in chunks). The Server guarantees that it will 
        /// not send duplicates in the chunks and that it will not send a 
        /// DeviceHearbeatUpdate until the comlete list with known Devices
        /// has been sent.
        /// </remarks>
        public Task DeviceStatusReport(DeviceStatusOutDTO[] statusAttributesList);
    }
}
