﻿using System;
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
    /// the server can deal with any of its historic variants.    /// 
    /// </remarks>
    public record MonToSrvRegistrationArgs : IHubArgs
    {
        // empty at this time
    }

    /// <summary>
    /// Hub Interface provided by The server side to the Monitor Clients
    /// </summary>
    /// <remarks>
    /// <para>
    /// The parameters for the Hub methods are encapsulated in a operation
    /// specific record type so that the interface can relatively easily
    /// be extended.
    /// </para>
    /// The real calling code can not be implemented with this interface as
    /// there currently is no method to generate Hub stubs and proxies 
    /// automatically from an interface definition. 
    /// But this still aids in documentation.
    /// </remarks>
    public interface IMonitorHubServer
    {
        /// <summary>
        /// The local path to the MonitorHub on the IoTAS Server 
        /// </summary>
        public const string path = "/monitor-hub";

        /// <summary>
        /// Register a Monitor in the Server.
        /// </summary>
        /// <param name="monitorRegistrationArgs"></param>
        /// <returns>A Task</returns>
        Task RegisterMonitorAsync(MonToSrvRegistrationArgs monitorRegistrationArgs);

    }
}