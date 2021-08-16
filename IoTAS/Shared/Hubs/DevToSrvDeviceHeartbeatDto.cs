//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// for a Device Heartbeat
    /// </summary>
    public sealed record DevToSrvDeviceHeartbeatDto(int DeviceId) : BaseHubInDto;
}
