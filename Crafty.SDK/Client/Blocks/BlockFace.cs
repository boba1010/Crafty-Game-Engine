using System.Numerics;

namespace Crafty.SDK.Client.Blocks;

public readonly record struct BlockFace(BlockFaceDirection Direction, string Texture);

public readonly record struct BlockElement(Vector3 Min, Vector3 Max, IReadOnlyList<BlockFace> Faces);