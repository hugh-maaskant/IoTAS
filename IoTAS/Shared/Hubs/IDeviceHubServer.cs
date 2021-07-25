using System;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// during Device Registration.
    /// </summary>
    public record DevToSrvDeviceRegistrationDto(int DeviceId) : HubInDto;
    

    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// for a Device Heartbeat
    /// </summary>
    public record DevToSrvDeviceHeartbeatDto(int DeviceId) : HubInDto;


    /// <summary>
    /// Hub Interface provided by the Server side to the Device Clients
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
        /// <param name="deviceRegistrationDto">The Device's registration arguments</param>
        /// <returns>A Task</returns>
        public Task RegisterDeviceClient(DevToSrvDeviceRegistrationDto deviceRegistrationDto);

        /// <summary>
        /// ReceiveDeviceHeartbeat message from a Device.   
        /// </summary>
        /// <param name="deviceHeartbeatDto">The Device' heartbeat arguments</param>
        /// <returns>A Task</returns>
        public Task ReceiveDeviceHeartbeat(DevToSrvDeviceHeartbeatDto deviceHeartbeatDto);
    }
}
