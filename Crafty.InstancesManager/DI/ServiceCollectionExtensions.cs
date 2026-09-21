using Microsoft.Extensions.DependencyInjection;

namespace Crafty.InstancesManager.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCraftyInstancesManager(
        this IServiceCollection services)
    {
        services.AddSingleton<IInstanceService, InstanceService>();

        return services;
    }
}