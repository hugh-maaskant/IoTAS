using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// during Device Registration.
    /// </summary>
    public record DeviceRegistrationInDTO : IHubInDTO
    {
        int deviceId;       // The Id ofthe Device to register in the IoTAS Server
    }

    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// for a Heartbeat message
    /// </summary>
    public record DeviceHeartbeatInDTO : IHubInDTO
    {
        int deviceId;       // The Id ofthe Device that sent the heartbeat
    }


    /// <summary>
    /// Hub Interface provided by The server side to the Device Clients
    /// </summary>
    public interface IDeviceHubServer
    {
        /// <summary>
        /// The local path to the DeviceHub on the IoTAS Server 
        /// </summary>
        public const string deviceHubPath = "/device";

        /// <summary>
        /// Register a Device in the Server.
        /// </summary>
        /// <param name="deviceRegistrationAttributes">The Device's registrationj attributes</param>
        /// <returns>A Task</returns>
        Task RegisterDevice(DeviceRegistrationInDTO deviceRegistrationAttributes);

        /// <summary>
        /// Heartbeat message from a Device.   
        /// </summary>
        /// <param name="deviceId">The Device Id as defined by the Device</param>
        /// <returns>A Task</returns>
        Task Heartbeat(DeviceHeartbeatInDTO deviceHeartbeatAttributes);
    }
}
