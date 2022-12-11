namespace SwitchBot.Api.Devices
{
    using InTheHand.Bluetooth;

    public class Bulb : SwitchBotDevice, IBulb
    {
        public Bulb(string id, ISwitchBotDeviceCommander deviceCommander)
            : base(id, DeviceType.Blub, deviceCommander)
        {
        }
    }
}
