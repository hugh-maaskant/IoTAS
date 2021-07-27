
namespace IoTAS.Shared.Hubs
{
    /// <summary>
    /// Device Attributes that need to be passed to the Server
    /// during Device Registration.
    /// </summary>
    public record DevToSrvDeviceRegistrationDto(int DeviceId) : BaseHubInDto;
}
