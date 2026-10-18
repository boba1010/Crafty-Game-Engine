using Crafty.ChunkGeneration.World;
using Crafty.Components;
using Crafty.Core;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Physics;

namespace Crafty.Systems;

public sealed class MobsSpawnSystem : IGameplaySystem
{
    private int _mobsCount;

    public void Update(ref Scene scene, double deltaTime)
    {
        if (_mobsCount > 0)
            return;

        const int spawnX = 7;
        const int spawnZ = 0;

        int chunkX = (int)MathF.Floor(spawnX / (float)Chunk.Size);
        int chunkZ = (int)MathF.Floor(spawnZ / (float)Chunk.Size);

        if (!WorldManager.Current.TryGetChunk(chunkX, chunkZ, out _))
            return;

        var zombieModel = BuildZombieModel(ref scene);
        var zombie = scene.CreateObject();

        scene.SetParent(zombieModel, zombie);
        scene.AddComponent(zombie, new Damageable());
        scene.AddComponent(zombie, new FallDamage());
        scene.AddComponent(zombie, new Attackable());
        scene.AddComponent(zombie, new Attacker()
        {
            Damage = 3,
            Range = 2,
            AttackSpeed = 1.5
        });
        scene.AddComponent(zombie, new AttackTimer());
        scene.AddComponent(zombie, new HostileMob());
        scene.AddComponent(zombie, new Transform
        {
            Position = new(spawnX, 70, spawnZ)
        });
        scene.AddComponent(zombie, new Movement());
        scene.AddComponent(zombie, new Collider(new(0.6f, 1.8f, 0.6f)));
        scene.AddComponent(zombie, new Gravity(40f));

        _mobsCount++;
    }

    private static WorldObject BuildZombieModel(ref Scene scene)
    {
        var mesh = MeshBuilder.BuildPlayerMesh();

        var zombieModel = scene.CreateObject();

        scene.AddComponent(zombieModel, new Transform
        {
            LockXAxis = true,
            LockZAxis = true,
            LocalPosition = new(0, -0.9f, 0)
        });

        scene.SetRenderable(zombieModel, new(mesh, true));

        return zombieModel;
    }
}
