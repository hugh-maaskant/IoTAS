//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs;

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
    /// <param name="dtoIn">The Device's registration parameters</param>
    /// <returns>A Task</returns>
    public Task RegisterDeviceClient(DevToSrvDeviceRegistrationDto dtoIn);

    /// <summary>
    /// ReceiveDeviceHeartbeat message from a Device.   
    /// </summary>
    /// <param name="dtoIn">The Device's heartbeat parameters</param>
    /// <returns>A Task</returns>
    public Task ReceiveDeviceHeartbeat(DevToSrvDeviceHeartbeatDto dtoIn);
}