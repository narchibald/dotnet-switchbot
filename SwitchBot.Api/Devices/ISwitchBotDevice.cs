namespace SwitchBot.Api.Devices
{
    public interface ISwitchBotDevice
    {
        string Id { get; }

        DeviceType DeviceType { get; }
    }
}