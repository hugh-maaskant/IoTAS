//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs;

/// <summary>
/// Hub Interface provided by The server side to the Monitor Clients
/// </summary>
/// <remarks>
/// The parameters for the Hub methods are encapsulated in a operation
/// specific record type so that the interface can relatively easily
/// be extended.
/// </remarks>
public interface IMonitorHubServer
{
    /// <summary>
    /// The local path to the MonitorHub on the IoTAS Server 
    /// </summary>
    public const string path = "/monitor-hub";

    /// <summary>
    /// Register a Monitor Client in the Server
    /// </summary>
    /// <param name="dtoIn">The received DTO as passed by SignalR</param>
    /// <returns>A Task</returns>
    Task RegisterMonitorClient(MonToSrvRegistrationDto dtoIn);
}