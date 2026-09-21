using Crafty.ChunkGeneration.World;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine;

public sealed class ChunkMeshBuilder
{
    private readonly List<float> _vertices = [];
    private readonly List<uint> _indices = [];

    private enum Face
    {
        Left,
        Right,
        Bottom,
        Top,
        Front,
        Back
    }

    public Mesh Build(Chunk chunk)
    {
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < 32; z++)
            {
                for (var y = 0; y < 400; y++)
                {
                    var block = chunk.GetBlock(x, y, z);

                    if (block.Id == 0)
                        continue;

                    if (x == 0 || chunk.GetBlock(x - 1, y, z).Id == 0)
                        AddFace(x, y, z, Face.Left);

                    if (x == Chunk.Size - 1 || chunk.GetBlock(x + 1, y, z).Id == 0)
                        AddFace(x, y, z, Face.Right);

                    if (y == 0 || chunk.GetBlock(x, y - 1, z).Id == 0)
                        AddFace(x, y, z, Face.Bottom);

                    if (y == 399 || chunk.GetBlock(x, y + 1, z).Id == 0)
                        AddFace(x, y, z, Face.Top);

                    if (z == 0 || chunk.GetBlock(x, y, z - 1).Id == 0)
                        AddFace(x, y, z, Face.Front);

                    if (z == Chunk.Size - 1 || chunk.GetBlock(x, y, z + 1).Id == 0)
                        AddFace(x, y, z, Face.Back);

                }
            }
        }

        return new Mesh([.. _vertices], [.. _indices]);
    }

    private void AddFace(int x, int y, int z, Face face)
    {
        var i = (uint)(_vertices.Count / 3);

        switch (face)
        {
            case Face.Left:
                AddVertex(x, y, z);
                AddVertex(x, y + 1, z);
                AddVertex(x, y + 1, z + 1);
                AddVertex(x, y, z + 1);
                break;

            case Face.Right:
                AddVertex(x + 1, y, z + 1);
                AddVertex(x + 1, y + 1, z + 1);
                AddVertex(x + 1, y + 1, z);
                AddVertex(x + 1, y, z);
                break;

            case Face.Bottom:
                AddVertex(x, y, z + 1);
                AddVertex(x, y, z);
                AddVertex(x + 1, y, z);
                AddVertex(x + 1, y, z + 1);
                break;

            case Face.Top:
                AddVertex(x, y + 1, z);
                AddVertex(x, y + 1, z + 1);
                AddVertex(x + 1, y + 1, z + 1);
                AddVertex(x + 1, y + 1, z);
                break;

            case Face.Front:
                AddVertex(x + 1, y, z);
                AddVertex(x + 1, y + 1, z);
                AddVertex(x, y + 1, z);
                AddVertex(x, y, z);
                break;

            case Face.Back:
                AddVertex(x, y, z + 1);
                AddVertex(x, y + 1, z + 1);
                AddVertex(x + 1, y + 1, z + 1);
                AddVertex(x + 1, y, z + 1);
                break;
        }

        _indices.Add(i);
        _indices.Add(i + 1);
        _indices.Add(i + 2);
        _indices.Add(i);
        _indices.Add(i + 2);
        _indices.Add(i + 3);
    }

    private void AddVertex(float x, float y, float z)
    {
        _vertices.Add(x);
        _vertices.Add(y);
        _vertices.Add(z);
    }
}
