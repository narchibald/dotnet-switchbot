namespace SwitchBot.Api
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::SwitchBot.Api.Devices;
    using InTheHand.Bluetooth;
    using Nito.AsyncEx;

    public class SwitchBot : ISwitchBot, ISwitchBotDeviceCommander
    {
        private static readonly ConcurrentDictionary<string, ISwitchBotDevice> DevicesValue = new ();

        public IReadOnlyList<ISwitchBotDevice> Devices => DevicesValue.Select(x => x.Value).ToList();

        public Task Discover() => FindDevices();

        public async Task<IBot?> GetBot(string id) => await FetchDevice(id) as IBot;

        public async Task<IBulb?> GetBlub(string id) => await FetchDevice(id) as IBulb;

        public async Task<ICurtain?> GetCurtain(string id) => await FetchDevice(id) as ICurtain;

        async Task<BluetoothDevice> ISwitchBotDeviceCommander.GetDevice(string id) => await BluetoothDevice.FromIdAsync(id);

        private async Task<ISwitchBotDevice?> FetchDevice(string id)
        {
            var bluetoothDevice = await ((ISwitchBotDeviceCommander)this).GetDevice(id);
            await RegisterDevice(bluetoothDevice);
            DevicesValue.TryGetValue(id, out var device);
            return device;
        }

        private async Task FindDevices()
        {
            var foundDevices = new ConcurrentDictionary<string, BluetoothDevice>();
            void AdvertisementReceivedHandler(object o, BluetoothAdvertisingEvent ef)
            {
                var device = ef.Device;
                foundDevices.AddOrUpdate(device.Id, (i) => device, (i, d) => device);
            }

            BluetoothLEScan? scan = null;
            try
            {
                using var cancellation = new CancellationTokenSource();
                Bluetooth.AdvertisementReceived += AdvertisementReceivedHandler;
                scan = await Bluetooth.RequestLEScanAsync();
                cancellation.CancelAfter(TimeSpan.FromSeconds(10));
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Bluetooth.AdvertisementReceived -= AdvertisementReceivedHandler;
                scan?.Stop();
            }

            // work out what devices are the switch bot ones
            foreach (var foundDevice in foundDevices)
            {
                RegisterDevice(foundDevice.Value);
            }
        }

        private async Task RegisterDevice(BluetoothDevice device)
        {
            var gatt = device.Gatt;

            await device.Gatt.ConnectAsync();
            try
            {
                var name = device.Name;
                ISwitchBotDevice? switchBotDevice = name switch
                {
                    "WoHand" => new Bot(device.Id, this),
                    "WoBulb" => new Bulb(device.Id, this),
                    "WoCurtain" => new Curtain(device.Id, this),
                    _ => null
                };

                if (switchBotDevice != null)
                {
                    DevicesValue.AddOrUpdate(device.Id, (i) => switchBotDevice, (i, d) => switchBotDevice);
                }
            }
            finally
            {
                if (gatt.IsConnected)
                {
                    gatt.Disconnect();
                }
            }
        }
    }
}