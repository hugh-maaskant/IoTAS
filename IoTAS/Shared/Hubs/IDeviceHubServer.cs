using System;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// during Device Registration.
    /// </summary>
    public record DevToSrvDeviceRegistrationArgs(int DeviceId) : HubInArgs;
    

    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// for a Device Heartbeat
    /// </summary>
    public record DevToSrvDeviceHeartbeatArgs(int DeviceId) : HubInArgs;


    /// <summary>
    /// Hub Interface provided by The server side to the Device Clients
    /// </summary>
    /// <remarks>
    /// The parameters for the Hub methods are encapsulated in a operation
    /// specific record type so that the interface can be extended
    /// </remarks>
    public interface IDeviceHubServer
    {
        /// <summary>
        /// The local path to the DeviceHub on the IoTAS Server 
        /// </summary>
        public const string path = "/device-hub";

        /// <summary>
        /// Register a Device in the Server.
        /// </summary>
        /// <param name="deviceRegistrationArgs">The Device's registration arguments</param>
        /// <returns>A Task</returns>
        public Task RegisterDeviceAsync(DevToSrvDeviceRegistrationArgs deviceRegistrationArgs);

        /// <summary>
        /// HandleHeartbeatAsync message from a Device.   
        /// </summary>
        /// <param name="deviceHeartbeatArgs">The Device' heartbeat arguments</param>
        /// <returns>A Task</returns>
        public Task HandleHeartbeatAsync(DevToSrvDeviceHeartbeatArgs deviceHeartbeatArgs);
    }
}
