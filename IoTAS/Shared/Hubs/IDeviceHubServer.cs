using System;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// during Device Registration.
    /// </summary>
    public record DevToSrvDeviceRegistrationArgs(int DeviceId) : IHubArgs;
    

    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// for a HandleHeartbeatAsync message
    /// </summary>
    public record DevToSrvDeviceHeartbeatArgs(int DeviceId) : IHubArgs;


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
