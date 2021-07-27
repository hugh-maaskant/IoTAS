using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Monitor Attributes that are passed to the Server during Registration.
    /// </summary>
    /// <remarks>
    /// This struct provides options to extend the 
    /// IMonitorHubServer.RegisterMonitor() operation with new data elements
    /// without being a breaking change on the interface itself (as long as
    /// the server can deal with any of its historic variants. 
    /// </remarks>
    public record MonToSrvRegistrationDto : BaseHubInDto
    {
        // empty at this time
    }

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
        /// Register a Monitor Client in the Server.
        /// </summary>
        /// <param name="dtoIn">The received DTA as passed by SignalR</param>
        /// <returns>A Task</returns>
        Task RegisterMonitorClient(MonToSrvRegistrationDto dtoIn);

    }
}