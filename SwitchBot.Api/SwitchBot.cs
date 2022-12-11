﻿namespace SwitchBot.Api
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

        public IReadOnlyList<ISwitchBotDevice> Devices => this.DevicesValue.Select(x => x.Value).ToList();

        public Task Discover() => FindDevices();

        public async Task<IBot?> GetBot(string id) => await FetchDevice(id) as IBot;

        public async Task<IBulb?> GetBlub(string id) => await FetchDevice(id) as IBulb;

        public async Task<ICurtain?> GetCurtain(string id) => await FetchDevice(id) as ICurtain;

        async Task<BluetoothDevice> ISwitchBotDeviceCommander.GetDevice(string id)
        {
            var wait = new AsyncManualResetEvent(false);
            BluetoothDevice? foundDevice = null;

            void AdvertisementReceivedHandler(object o, BluetoothAdvertisingEvent ef)
            {
                var device = ef.Device;
                if (device.Id == id)
                {
                    foundDevice = device;
                    wait.Set();
                }
            }

            BluetoothLEScan? scan = null;
            try
            {
                using var cancellation = new CancellationTokenSource();
                Bluetooth.AdvertisementReceived += AdvertisementReceivedHandler;
                scan = await Bluetooth.RequestLEScanAsync();
                cancellation.CancelAfter(TimeSpan.FromSeconds(30));
                await wait.WaitAsync(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Bluetooth.AdvertisementReceived -= AdvertisementReceivedHandler;
                scan?.Stop();
            }

            if (foundDevice == null)
            {
                throw new DeviceNotFoundException(id);
            }

            return foundDevice;
        }

        private async Task<ISwitchBotDevice?> FetchDevice(string id)
        {
            await FindDevices(id);
            this.DevicesValue.TryGetValue(id, out var device);
            return device;
        }

        private async Task FindDevices(string? id = null)
        {
            if (id != null && this.DevicesValue.ContainsKey(id))
            {
                return;
            }

            var wait = new AsyncManualResetEvent(false);
            var foundDevices = new ConcurrentDictionary<string, BluetoothDevice>();
            void AdvertisementReceivedHandler(object o, BluetoothAdvertisingEvent ef)
            {
                var device = ef.Device;
                foundDevices.AddOrUpdate(device.Id, (i) => device, (i, d) => device);
                if (id != null && device.Id == id)
                {
                    wait.Set();
                }
            }

            BluetoothLEScan? scan = null;
            try
            {
                using var cancellation = new CancellationTokenSource();
                Bluetooth.AdvertisementReceived += AdvertisementReceivedHandler;
                scan = await Bluetooth.RequestLEScanAsync();
                cancellation.CancelAfter(TimeSpan.FromSeconds(10));
                await wait.WaitAsync(cancellation.Token);
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
                var device = foundDevice.Value;
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
                        this.DevicesValue.AddOrUpdate(device.Id, (i) => switchBotDevice, (i, d) => switchBotDevice);
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
}