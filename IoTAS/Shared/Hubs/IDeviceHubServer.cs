using System;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// during Device Registration.
    /// </summary>
    public record DeviceRegistrationInDTO(int DeviceId) : IHubEvent;
    

    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// for a Heartbeat message
    /// </summary>
    public record DeviceHeartbeatInDTO(int DeviceId) : IHubEvent;


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
        public const string path = "/device";

        /// <summary>
        /// Register a Device in the Server.
        /// </summary>
        /// <param name="deviceRegistrationAttributes">The Device's registrationj attributes</param>
        /// <returns>A Task</returns>
        public Task RegisterDevice(DeviceRegistrationInDTO deviceRegistrationAttributes);

        /// <summary>
        /// Heartbeat message from a Device.   
        /// </summary>
        /// <param name="deviceId">The Device Id as defined by the Device</param>
        /// <returns>A Task</returns>
        public Task Heartbeat(DeviceHeartbeatInDTO deviceHeartbeatAttributes);
    }
}
