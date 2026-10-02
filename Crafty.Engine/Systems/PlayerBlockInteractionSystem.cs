using Crafty.ChunkGeneration.World;
using Crafty.Engine.Components;
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
    public float PlaceCooldown { get; set; } = 0.15f;
    private float _placeCooldown;

    public void Update(ref Scene scene, double deltaTime)
    {
        if (GameStateManager.IsPaused || GameStateManager.IsInventory)
            return;

        var cameraObject = scene.GetEntitiesWith<Camera>().FirstOrDefault();
        var playerObject = scene.GetEntitiesWith<Player>().FirstOrDefault();

        ref var camera = ref scene.GetComponent<Camera>(cameraObject);
        ref var cameraTransform = ref scene.GetComponent<Transform>(cameraObject);

        var cameraPosition = cameraTransform.Position;
        float pitch = cameraTransform.Rotation.X;
        float yaw = cameraTransform.Rotation.Y;

        Vector3 direction = new(-MathF.Cos(pitch) * MathF.Sin(yaw), MathF.Sin(pitch), -MathF.Cos(pitch) * MathF.Cos(yaw));

        if (SystemAPI.Input.IsMouseButtonDown(MouseButton.Left))
            HandleBlockBreak((float)deltaTime, cameraPosition, direction);

        if (SystemAPI.Input.IsMouseButtonDown(MouseButton.Right))
        {
            ref var hotbar = ref scene.GetComponent<Hotbar>(playerObject);
            HandleBlockPlace((float)deltaTime, cameraPosition, direction, hotbar.Slots[hotbar.SelectedSlot]);
        }
    }

    private void HandleBlockBreak(float deltaTime, Vector3 cameraPosition, Vector3 direction)
    {
        _breakCooldown -= deltaTime;

        if (_breakCooldown > 0)
            return;

        if (!VoxelRaycast.Raycast(cameraPosition, direction, Reach, IsSolid, out var hit))
            return;

        world.SetBlock(hit.X, hit.Y, hit.Z, 0);
        _breakCooldown = BreakCooldown;
    }

    private void HandleBlockPlace(float deltaTime, Vector3 cameraPosition, Vector3 direction, InventorySlot slot)
    {
        _placeCooldown -= deltaTime;

        if (_placeCooldown > 0)
            return;

        if (!VoxelRaycast.Raycast(cameraPosition, direction, Reach, IsSolid, out var hit))
            return;

        var x = hit.X + (int)hit.Normal.X;
        var y = hit.Y + (int)hit.Normal.Y;
        var z = hit.Z + (int)hit.Normal.Z;

        if (slot.BlockId.HasValue)
            world.SetBlock(x, y, z, slot.BlockId.Value);
        _placeCooldown = PlaceCooldown;
    }

    private bool IsSolid(int x, int y, int z)
    {
        var block = world.GetBlock(x, y, z);
        return block.Id != 0;
    }
}
