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
    public record Request(DateTime ReceivedAt, IHubInDTO Data)
    {

    }
    

    
}
