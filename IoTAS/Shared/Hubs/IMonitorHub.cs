//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Typesafe Hub Interface for the Server to remote call a Monitor Client
    /// </summary>
    public interface IMonitorHub
    {
        /// <summary>
        /// An RPC to send Device Status to a Monitor,
        /// e.g. due to a received DeviceRegistration
        /// </summary>
        /// <param name="statusDto">The Device status attributes</param>
        /// <returns>A Task</returns>
        public Task ReceiveDeviceStatusUpdate(SrvToMonDeviceStatusDto statusDto);

        /// <summary>
        /// An RPC to notify Monitor(s) of a received Device Heartbeat
        /// </summary>
        /// <param name="hearbeatDto">Heartbead DTO</param>
        /// <returns>A Task</returns>
        public Task ReceiveDeviceHeartbeatUpdate(SrvToMonDeviceHeartbeatDto hearbeatDto);
        
        /// <summary>
        /// An RPC to send multiple Device statuses to a Monitor,
        /// e.g. due to a received Monitor registration
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
        public Task ReceiveDeviceStatusesSnapshot(SrvToMonDeviceStatusDto[] statusListDto);
    }
}
