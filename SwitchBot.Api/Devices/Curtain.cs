namespace SwitchBot.Api.Devices
{
    using InTheHand.Bluetooth;

    public class Curtain : SwitchBotDevice, ICurtain
    {
        public Curtain(string id, ISwitchBotDeviceCommander deviceCommander)
            : base(id, DeviceType.Curtain, deviceCommander)
        {
        }
    }
}
