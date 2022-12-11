namespace SwitchBot.Api.Devices
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using InTheHand.Bluetooth;

    public class Bot : SwitchBotDevice, IBot
    {
        private static readonly BluetoothUuid ServiceId = BluetoothUuid.FromGuid(Guid.Parse("cba20d00-224d-11e6-9fb8-0002a5d5c51b"));
        private static readonly byte[] PressCommand = { 0x57, 0x01, 0x00 };

        public Bot(string id, ISwitchBotDeviceCommander deviceCommander)
         : base(id, DeviceType.Bot, deviceCommander)
        {
        }

        public Task<bool> Press() => ExecuteCommand(PressCommand);

        private async Task<bool> ExecuteCommand(byte[] command)
        {
            var device = await this.GetDevice();
            var gatt = device.Gatt;
            await gatt.ConnectAsync();

            try
            {
                var service = await gatt.GetPrimaryServiceAsync(ServiceId);
                var characteristics = await service.GetCharacteristicsAsync();
                var characteristic = characteristics.First(c => c.Properties.HasFlag(GattCharacteristicProperties.Write));
                await characteristic.WriteValueWithResponseAsync(command);
            }
            finally
            {
                if (gatt.IsConnected)
                {
                    gatt.Disconnect();
                }
            }

            return true;
        }
    }
}
