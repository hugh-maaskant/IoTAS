using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTAS.Monitor.Services
{

    public interface IConnectionStatus
    {
        public bool Up { get; }

        public string ConnectionId { get; }

        public DateTime ConnectedAt { get; }
    }
}
