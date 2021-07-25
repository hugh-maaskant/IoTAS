  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    public record SrvToMonDeviceHeartbeatDto
    (
        int      DeviceId,          // The Id of the Device
        DateTime ReceivedAt         // The DateTime that the HeartBeat was received
    );

    public record SrvToMonDeviceStatusDto
    (
        int      DeviceId,          // The Id of the Device
        DateTime FirstRegisteredAt, // The very first registration DateTime
        DateTime LastRegisteredAt,  // The most recent registration DateTime
        DateTime LastSeenAt         // The most recently seen registration or heartbeat DateTime
    );

    /// <summary>
    /// Typesafe Hub Interface for the IoTAS Server to signal (call) the Monitor Clients
    /// </summary>
    public interface IMonitorHub
    {
        /// <summary>
        /// A remote call to notify Monitor(s) of a received Device ReceiveDeviceHeartbeat
        /// </summary>
        /// <param name="hearbeatDto">Heartbead DTO</param>
        /// <returns>A Task</returns>
        public Task ReceiveDeviceHeartbeatUpdate(SrvToMonDeviceHeartbeatDto hearbeatDto);

        /// <summary>
        /// A remote call to notify Monitor(s) of a received DeviceRegistration
        /// </summary>
        /// <param name="statusDto">The Device status attributes</param>
        /// <returns>A Task</returns>
        public Task ReceiveDeviceRegistrationUpdate(SrvToMonDeviceStatusDto statusDto);

        /// <summary>
        /// A remote call to inform a Monitor of the current Devices status
        /// </summary>
        /// <param name="statusListDto">A List of Device status DTOs</param>
        /// <returns>A Task</returns>
        /// <remarks>
        /// When there are too many Devices, the server may send this message 
        /// multiple times (i.e. in chunks). The Server must guarantee that it 
        /// will not send duplicates in the chunks and that it will not send a 
        /// DeviceHearbeatUpdate until the comlete list with known Devices has 
        /// been sent.
        /// </remarks>
        public Task ReceiveDeviceStatusesReport(SrvToMonDeviceStatusDto[] statusListDto);
    }
}
