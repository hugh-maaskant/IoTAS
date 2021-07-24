using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTAS.Server.DevicesStatusStore
{
    /// <summary>
    /// Interface for the internal Device (Reporting) Status Store
    /// </summary>
    /// <remarks>
    /// At this point in time it is minimal i.e. does not provide creation or 
    /// deletion and probably some other operations.
    /// 
    /// Therefore the implementation can bbe very permissive and implicitly create
    /// an entry if a new deviceId is upated (either with a registration or a
    /// heartbeat command.
    /// 
    /// This is just to get through milestone 1 :-)
    /// </remarks>
    internal interface IDeviceStatusStore
    {
        /// <summary>
        /// Get the current DeviceReportingStatus for the Device with DeviceId deviceId
        /// </summary>
        /// <param name="deviceId">The Id of the Device to get the status off</param>
        /// <returns>A DeviceReportingStatus record</returns>
        internal DeviceReportingStatus GetDeviceStatus(int deviceId);

        /// <summary>
        /// Get the current DeviceReportingStatus for all the Devices
        /// </summary>
        /// <returns>An enumerable copy of the current DeviceReportingStatus of all Devices</returns>
        internal IEnumerable<DeviceReportingStatus> GetDeviceStatuses();

        /// <summary>
        /// Upate the Device Registration date and time for the Device with DeviceId deviceId.
        /// </summary>
        /// <param name="deviceId">The Id of the Device to update</param>
        /// <param name="receivedAt">The date and time the Registration was received</param>
        internal void UpdateRegistration(int deviceId, DateTime receivedAt);

        /// <summary>
        /// Upate the Device Heartbeat date and time for the Device with DeviceId deviceId
        /// </summary>
        /// <param name="deviceId">The Id of the Device to update</param>
        /// <param name="receivedAt">The date and time the HHeartbeat was received</param>
        internal void UpateHeartbeat(int deviceId, DateTime receivedAt);
    }
}
