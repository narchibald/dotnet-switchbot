namespace SwitchBot.Api.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwitchBotServices(this IServiceCollection services)
        {
            services.AddSingleton<ISwitchBot, SwitchBot>();

            return services;
        }
    }
}
