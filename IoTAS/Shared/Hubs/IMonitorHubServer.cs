using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Monitor Attributes that can optionally be passed to the Server
    /// during Monitor Registration.
    /// </summary>
    /// <remarks>
    /// This struct provides options to extend the 
    /// IMonitorHubServer.RegisterMonitor() operation with new data elements
    /// without being a breaking change on the interface itself (as long as
    /// the server can deal with any of its historic variants.    /// 
    /// </remarks>
    public record MonitorRegistrationInDTO : IHubInDTO
    {
        // empty at this time
    }

    /// <summary>
    /// Hub Interface provided by The server side to the Monitor Clients
    /// </summary>
    public interface IMonitorHubServer
    {
        /// <summary>
        /// The local path to the MonitorHub on the IoTAS Server 
        /// </summary>
        public const string monitorHubPath = "/monitor";

        /// <summary>
        /// Register a Monitor in the Server.
        /// </summary>
        /// <param name="monitorRegistrationAttributes"></param>
        /// <param name="deviceAttributes">Optional device attributes</param>
        /// <returns>A Task</returns>
        Task RegisterMonitor(MonitorRegistrationInDTO monitorRegistrationAttributes);

    }
}