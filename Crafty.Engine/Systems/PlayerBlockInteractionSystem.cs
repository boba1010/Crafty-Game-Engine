using Crafty.ChunkGeneration.World;
using Crafty.Engine.Helpers;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Physics;
using System.Numerics;
namespace Crafty.Engine.Systems;

public sealed class PlayerBlockInteractionSystem(World world) : ISystem
{
    public float Reach { get; set; } = 6f;
    public float BreakCooldown { get; set; } = 0.15f;
    private float _breakCooldown;

    public void Update(ref Scene scene, double deltaTime)
    {
        _breakCooldown -= (float)deltaTime;

        foreach (var entity in scene.GetEntitiesWith<Camera>())
        {
            ref var camera = ref scene.GetComponent<Camera>(entity);
            ref var cameraTransform = ref scene.GetComponent<Transform>(entity);

            var cameraPosition = cameraTransform.Position;
            float pitch = cameraTransform.Rotation.X;
            float yaw = cameraTransform.Rotation.Y;

            Vector3 direction = new(-MathF.Cos(pitch) * MathF.Sin(yaw), MathF.Sin(pitch), -MathF.Cos(pitch) * MathF.Cos(yaw));

            if (!SystemAPI.Input.IsMouseButtonDown(MouseButton.Left))
                continue;

            if (_breakCooldown > 0)
                continue;

            if (!VoxelRaycast.Raycast(cameraPosition, direction, Reach, IsSolid, out var hit))
                continue;

            world.SetBlock(hit.X, hit.Y, hit.Z, 0);
            _breakCooldown = BreakCooldown;
        }
    }

    private bool IsSolid(int x, int y, int z)
    {
        var block = world.GetBlock(x, y, z);
        return block.Id != 0;
    }
}
