using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;

namespace Crafty.Engine.Core;

public static class WorldManager
{
    public static World Load()
    {
        var worldPath = Program.LaunchConfig.SelectedWorldPath;
        return WorldIOManager.LoadWorld(worldPath);
    }

    public static void Save()
    {

    }
}
