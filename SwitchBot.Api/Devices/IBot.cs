namespace SwitchBot.Api.Devices
{
    using System.Threading.Tasks;

    public interface IBot : ISwitchBotDevice
    {
        Task<bool> Press();
    }
}