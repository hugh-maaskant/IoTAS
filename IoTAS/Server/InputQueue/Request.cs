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
    public record Request(DateTime ReceivedAt, IHubArgs Data)
    {
        /// <summary>
        /// Create a Request from an IHubArgs
        /// </summary>
        /// <param name="data">The input hub event</param>
        /// <returns>A Request record</returns>
        public static Request FromInDTO(IHubArgs data)
        {
            return new Request(DateTime.Now, data);
        }
    }  
}
