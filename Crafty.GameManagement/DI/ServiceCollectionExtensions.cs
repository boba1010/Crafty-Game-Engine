using Microsoft.Extensions.DependencyInjection;

namespace Crafty.GameManagement.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCraftyGameManagement(this IServiceCollection services)
    {
        services.AddSingleton<ILaunchDataService, LaunchDataService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IWorldService, WorldService>();

        return services;
    }
}
