using Microsoft.Extensions.DependencyInjection;

namespace Crafty.ChunkGeneration.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCraftyChunkGen(this IServiceCollection services)
    {
        services.AddSingleton<IWorldGenService, WorldGenService>();

        return services;
    }
}
