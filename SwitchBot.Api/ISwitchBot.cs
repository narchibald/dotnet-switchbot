namespace SwitchBot.Api
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::SwitchBot.Api.Devices;

    public interface ISwitchBot
    {
        IReadOnlyList<ISwitchBotDevice> Devices { get; }

        Task Discover();

        Task<IBot?> GetBot(string id);

        Task<IBulb?> GetBlub(string id);

        Task<ICurtain?> GetCurtain(string id);
    }
}