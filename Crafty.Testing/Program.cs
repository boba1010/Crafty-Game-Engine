using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.World;
using Crafty.SDK.World;
using System.Runtime.CompilerServices;

namespace Crafty.Testing;

internal class Program
{
    public static void Main()
    {
        Console.WriteLine(Unsafe.SizeOf<BlockPlacement>());
    }
}
