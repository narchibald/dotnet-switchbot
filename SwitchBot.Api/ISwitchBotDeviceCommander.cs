namespace SwitchBot.Api
{
    using System.Threading.Tasks;
    using InTheHand.Bluetooth;

    public interface ISwitchBotDeviceCommander
    {
        Task<BluetoothDevice> GetDevice(string id);
    }
}