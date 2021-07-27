using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// for a Device Heartbeat
    /// </summary>
    public record DevToSrvDeviceHeartbeatDto(int DeviceId) : BaseHubInDto;
}
