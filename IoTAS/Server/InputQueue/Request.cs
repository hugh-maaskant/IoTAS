using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTAS.Shared.Hubs;

namespace IoTAS.Server.InputQueue
{
    /// <summary>
    /// The Request record that gets queued on input from (i.e. call to) a Hub method
    /// </summary>
    /// <remarks>
    /// The Request record contains any and all information needed to process the request
    /// </remarks>
    public record Request(DateTime ReceivedAt, string ConnectionId, HubInDto ReceivedData)
    {
        /// <summary>
        /// Create a Request from an incoming Hub call
        /// </summary>
        /// <param name="connectionId">The incoming connectionId</param>
        /// <param name="receivedData">The incoming hub event arguments</param>
        /// <returns>A Request record</returns>
        public static Request FromClientCall(string connectionId, HubInDto receivedData)
        {
            return new Request(DateTime.Now, connectionId, receivedData);
        }
    }  
}
