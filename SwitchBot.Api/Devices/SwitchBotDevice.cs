using System.Threading.Tasks;

namespace SwitchBot.Api.Devices
{
    using InTheHand.Bluetooth;

    public abstract class SwitchBotDevice : ISwitchBotDevice
    {
        private readonly ISwitchBotDeviceCommander deviceCommander;

        protected internal SwitchBotDevice(string id, DeviceType deviceType, ISwitchBotDeviceCommander deviceCommander)
        {
            this.deviceCommander = deviceCommander;
            this.Id = id;
            this.DeviceType = deviceType;
        }

        public string Id { get; }

        public DeviceType DeviceType { get; }

        protected Task<BluetoothDevice> GetDevice() => this.deviceCommander.GetDevice(this.Id);
    }
}
