//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

namespace IoTAS.Shared.Hubs;

/// <summary>
/// Device Attributes that need to be passed to the Server
/// during Device Registration.
/// </summary>
public sealed record DevToSrvDeviceRegistrationDto(int DeviceId) : BaseHubInDto;