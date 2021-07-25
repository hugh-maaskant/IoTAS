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
    /// The implementation needs to implicitly create an entry if an
    /// unknown (i.e. not seen before) deviceId is either updated with 
    /// a registration or a heartbeat command. As a result the result(s)
    /// are always "truthfull", with DateTime.MinValue use as "never".
    /// </remarks>
    public interface IDeviceStatusStore
    {
        /// <summary>
        /// Get the current DeviceReportingStatus for the Device with DeviceId deviceId
        /// </summary>
        /// <param name="deviceId">The Id of the Device to get the status off</param>
        /// <returns>A DeviceReportingStatus record</returns>
        public DeviceReportingStatus GetDeviceStatus(int deviceId);

        /// <summary>
        /// Get the current DeviceReportingStatus for all the Devices
        /// </summary>
        /// <returns>An enumerable copy (snapshot) of the current DeviceReportingStatus of all Devices</returns>
        public IEnumerable<DeviceReportingStatus> GetDeviceStatuses();

        /// <summary>
        /// Upate the Device Registration date and time for the Device with DeviceId deviceId.
        /// </summary>
        /// <param name="deviceId">The Id of the Device to update</param>
        /// <param name="receivedAt">The date and time the Registration was received</param>
        /// <returns>The new or updated <see cref="DeviceReportingStatus"/> record</returns>
        public DeviceReportingStatus UpdateRegistration(int deviceId, DateTime receivedAt);

        /// <summary>
        /// Upate the Device Heartbeat date and time for the Device with DeviceId deviceId
        /// </summary>
        /// <param name="deviceId">The Id of the Device to update</param>
        /// <param name="receivedAt">The date and time the HHeartbeat was received</param>
        /// <returns>The new or updated <see cref="DeviceReportingStatus"/> record</returns>
        public DeviceReportingStatus UpdateHeartbeat(int deviceId, DateTime receivedAt);
    }
}
