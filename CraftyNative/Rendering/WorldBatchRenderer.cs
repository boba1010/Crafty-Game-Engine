using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.World;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace CraftyNative.Rendering;

public sealed class WorldBatchRenderer(MeshRenderer meshRenderer) : IDisposable
{
    private readonly Dictionary<(int X, int Z), ChunkBatch> _batches = [];
    private readonly List<ChunkBatch> _drawList = [];
    private readonly List<(float Dist, ChunkBatch Batch, WorldMeshManager.ChunkMeshSet Set, int Version)> _stale = [];
    private readonly List<(float Dist, ChunkBatch Batch)> _translucentOrder = [];

    public double BuildBudgetMs { get; set; } = 2.0;

    // Call once per frame, before DrawOpaque.
    public void Update(Vector3 cameraPos)
    {
        while (WorldMeshManager.TryDequeueRemovedChunk(out var removed))
        {
            if (_batches.Remove(removed, out var old))
            {
                meshRenderer.Release(old.Opaque);
                meshRenderer.Release(old.Translucent);
            }
        }

        _drawList.Clear();
        _stale.Clear();

        foreach (var (key, set) in WorldMeshManager.Chunks)
        {
            if (!_batches.TryGetValue(key, out var batch))
                _batches[key] = batch = new ChunkBatch(key);

            _drawList.Add(batch);

            int version = Volatile.Read(ref set.Version);   // read before the sections

            if (batch.Version != version)
                _stale.Add((DistanceSquared(key, cameraPos), batch, set, version));
        }

        // nearest stale chunks first, rebuilt under a frame budget
        _stale.Sort(static (a, b) => a.Dist.CompareTo(b.Dist));

        long start = Stopwatch.GetTimestamp();

        for (int i = 0; i < _stale.Count; i++)
        {
            // always allow one per frame so work keeps progressing
            if (i > 0 && Stopwatch.GetElapsedTime(start).TotalMilliseconds >= BuildBudgetMs)
                break;

            var (_, batch, set, version) = _stale[i];
            batch.Build(set.Sections, version);
        }
    }

    public void DrawOpaque(Matrix4x4 mvp, Vector4 clip)
    {
        foreach (var batch in _drawList)
        {
            if (batch.Opaque.Indices.Count > 0)
                meshRenderer.Draw(batch.Opaque, mvp, clip: clip);
        }
    }

    // Call after SetPipeline(TranslucentPipeline).
    public void DrawTranslucent(Matrix4x4 mvp, Vector4 clip, Vector3 cameraPos)
    {
        _translucentOrder.Clear();

        foreach (var batch in _drawList)
        {
            if (batch.Translucent.Indices.Count > 0)
                _translucentOrder.Add((DistanceSquared(batch.Key, cameraPos), batch));
        }

        _translucentOrder.Sort(static (a, b) => b.Dist.CompareTo(a.Dist));   // far to near

        foreach (var (_, batch) in _translucentOrder)
            meshRenderer.Draw(batch.Translucent, mvp, clip: clip);
    }

    public void InvalidateAll()
    {
        foreach (var batch in _batches.Values)
            batch.Version = -1;
    }

    public void Dispose()
    {
        foreach (var batch in _batches.Values)
        {
            meshRenderer.Release(batch.Opaque);
            meshRenderer.Release(batch.Translucent);
        }

        _batches.Clear();
        _drawList.Clear();
    }

    private static float DistanceSquared((int X, int Z) key, Vector3 cameraPos)
    {
        float size = SectionCoordinate.ChunkSize;
        var center = new Vector3(key.X * size + size * 0.5f, cameraPos.Y, key.Z * size + size * 0.5f);
        return Vector3.DistanceSquared(center, cameraPos);
    }

    private sealed class ChunkBatch((int X, int Z) key)
    {
        public readonly (int X, int Z) Key = key;
        public readonly Mesh Opaque = new([], [], vertexStride: 24);
        public readonly Mesh Translucent = new([], [], vertexStride: 24);   // change if your translucent stride differs
        public int Version = -1;

        public void Build(WorldMeshSection[] sections, int version)
        {
            BuildInto(Opaque, sections, translucent: false);
            BuildInto(Translucent, sections, translucent: true);
            Version = version;
        }

        private static void BuildInto(Mesh dst, WorldMeshSection[] sections, bool translucent)
        {
            int vTotal = 0, iTotal = 0;

            foreach (var s in sections)
            {
                var m = translucent ? s.TranslucentMesh : s.Mesh;
                if (m is null || m.Vertices.Count == 0 || m.Indices.Count == 0)
                    continue;

                if (m.VertexStride != 24)
                    throw new InvalidOperationException("Incompatible vertex layout.");

                vTotal += m.Vertices.Count;
                iTotal += m.Indices.Count;
            }

            dst.Vertices.Clear();
            dst.Indices.Clear();
            CollectionsMarshal.SetCount(dst.Vertices, vTotal);
            CollectionsMarshal.SetCount(dst.Indices, iTotal);

            var dstV = CollectionsMarshal.AsSpan(dst.Vertices);
            var dstI = CollectionsMarshal.AsSpan(dst.Indices);

            int vo = 0, io = 0;
            uint baseVertex = 0;
            bool hasTexture = false;

            foreach (var s in sections)
            {
                var m = translucent ? s.TranslucentMesh : s.Mesh;
                if (m is null || m.Vertices.Count == 0 || m.Indices.Count == 0)
                    continue;

                if (!hasTexture)
                {
                    dst.Texture = m.Texture;
                    hasTexture = true;
                }
                else if (dst.Texture != m.Texture)
                    throw new InvalidOperationException("Cannot combine different textures.");

                var srcV = CollectionsMarshal.AsSpan(m.Vertices);
                var srcI = CollectionsMarshal.AsSpan(m.Indices);
                var p = s.Coordinate.WorldPosition;

                for (int v = 0; v < srcV.Length; v += 6)
                {
                    dstV[vo++] = srcV[v] + p.X;
                    dstV[vo++] = srcV[v + 1] + p.Y;
                    dstV[vo++] = srcV[v + 2] + p.Z;
                    dstV[vo++] = srcV[v + 3];
                    dstV[vo++] = srcV[v + 4];
                    dstV[vo++] = srcV[v + 5];
                }

                for (int i = 0; i < srcI.Length; i++)
                    dstI[io++] = srcI[i] + baseVertex;

                baseVertex += (uint)(srcV.Length / 6);
            }

            if (!hasTexture)
                dst.Texture = null;

            dst.MarkDirty();
        }
    }
}