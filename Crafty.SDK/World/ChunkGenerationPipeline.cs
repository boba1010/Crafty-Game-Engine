namespace Crafty.SDK.World;

public sealed class ChunkGenerationPipeline
{
    private static readonly Dictionary<ChunkGenerationStage, List<IChunkGenerator>> _generators = [];

    private static readonly ChunkGenerationStage[] Stages =
    [
        ChunkGenerationStage.Terrian,
        ChunkGenerationStage.Biome,
        ChunkGenerationStage.Surface,
        ChunkGenerationStage.Carvers,
        ChunkGenerationStage.Features,
        ChunkGenerationStage.Structures,
        ChunkGenerationStage.Ores,
        ChunkGenerationStage.PostProcess,
    ];

    public void Register(ChunkGenerationStage stage, IChunkGenerator generator)
    {
        if (!_generators.TryGetValue(stage, out var list))
        {
            list = [];
            _generators[stage] = list;
        }

        list.Add(generator);
    }

    public IChunk Generate(in ChunkGenerationContext context)
    {
        foreach (var stage in Stages)
        {
            if (!_generators.TryGetValue(stage, out var generators))
                continue;

            foreach (var generator in generators)
                generator.Generate(context);
        }

        throw new Exception();
    }
}
