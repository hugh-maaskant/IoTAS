//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Collections.Generic;

namespace IoTAS.Shared.DevicesStatusStore
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
        /// The number of <see cref="DeviceReportingStatus"/> items in the Store
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Remove all keys and <see cref="DeviceReportingStatus"/> items from the Store
        /// </summary>
        void Clear();

        /// <summary>
        /// Get the current DeviceReportingStatus for the Device with DeviceId deviceId
        /// </summary>
        /// <param name="deviceId">The Id of the Device to get the status off</param>
        /// <returns>A DeviceReportingStatus record</returns>
        public DeviceReportingStatus GetDeviceStatus(int deviceId);

        /// <summary>
        /// Get the current DeviceReportingStatus for all the Devices
        /// </summary>
        /// <returns>An IEnumerable<DeviceReportingStatus> over a copy (snapshot) of the current Devices statusses</returns>
        public IEnumerable<DeviceReportingStatus> GetDevicesStatusList();

        /// <summary>
        /// Insert or update the <see cref="DeviceReportingStatus"/> in the store
        /// </summary>
        /// <param name="">The DeviceReportingStatus to store</param>
        public void SetDeviceStatus(DeviceReportingStatus status);

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
        /// <remarks>
        /// If the entry does not exist a new entry is created based on the input, but this 
        /// shouldd not happen as thhe Device should first be registered.
        /// </remarks>
        public DeviceReportingStatus UpdateHeartbeat(int deviceId, DateTime receivedAt);
    }
}
