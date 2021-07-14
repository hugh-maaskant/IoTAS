using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTAS.Shared.Hubs;

namespace IoTAS.Server.InputQueue
{
    /// <summary>
    /// The Request record that gets queued on input from a Hub
    /// </summary>
    public record Request(DateTime ReceivedAt, IHubEvent Data)
    {
        /// <summary>
        /// Create a Request from an IHubEvent
        /// </summary>
        /// <param name="data">The input hub event</param>
        /// <returns>A Request record</returns>
        public static Request FromInDTO(IHubEvent data)
        {
            return new Request(DateTime.Now, data);
        }
    }  
}
