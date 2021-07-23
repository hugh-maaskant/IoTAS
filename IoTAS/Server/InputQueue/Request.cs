using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTAS.Shared.Hubs;

namespace IoTAS.Server.InputQueue
{
    /// <summary>
    /// The Request record that gets queued on input from a Hubup 
    /// </summary>
    public record Request(DateTime ReceivedAt, IHubArgs ReceivedData)
    {
        /// <summary>
        /// Create a Request from an IHubArgs
        /// </summary>
        /// <param name="receivedData">The input hub event data</param>
        /// <returns>A Request record</returns>
        public static Request FromInDTO(IHubArgs receivedData)
        {
            return new Request(DateTime.Now, receivedData);
        }
    }  
}
