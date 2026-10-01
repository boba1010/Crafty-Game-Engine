namespace Crafty.ChunkGeneration.World;

public readonly record struct BlockChanged(int X, int Y, int Z, ushort OldId, ushort NewId);
