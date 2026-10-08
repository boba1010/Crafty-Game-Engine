using Crafty.ChunkGeneration.World;
using Crafty.Engine.Components;
using Crafty.Engine.Core;
using Crafty.SDK.Client.Blocks;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Physics;
using System.Numerics;

namespace Crafty.Engine.Systems;

public sealed class PlayerBlockInteractionSystem(World world) : IGameplaySystem
{
    public float Reach { get; set; } = 6f;

    public float BreakCooldown { get; set; } = 0.15f;
    private float _breakCooldown;

    public float PlaceCooldown { get; set; } = 0.10f;
    private float _placeCooldown;

    public void Update(ref Scene scene, double deltaTime)
    {
        if (GameStateManager.IsInventory)
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

            HandleBlockPlace((float)deltaTime, cameraPosition, direction, yaw, hotbar.Slots[hotbar.SelectedSlot]);
        }
    }

    private void HandleBlockBreak(float deltaTime, Vector3 cameraPosition, Vector3 direction)
    {
        _breakCooldown -= deltaTime;

        if (_breakCooldown > 0)
            return;

        if (!VoxelRaycast.Raycast(cameraPosition, direction, Reach, IsSolid, out var hit))
            return;

        world.SetBlock(hit.X, hit.Y, hit.Z, 0, 0);

        _breakCooldown = BreakCooldown;
    }

    private void HandleBlockPlace(float deltaTime, Vector3 cameraPosition, Vector3 direction, float yaw, InventorySlot slot)
    {
        _placeCooldown -= deltaTime;

        if (_placeCooldown > 0)
            return;

        if (!VoxelRaycast.Raycast(cameraPosition, direction, Reach, IsSolid, out var hit))
            return;

        var x = hit.X + (int)hit.Normal.X;
        var y = hit.Y + (int)hit.Normal.Y;
        var z = hit.Z + (int)hit.Normal.Z;

        if (!slot.BlockId.HasValue)
            return;

        uint blockId = slot.BlockId.Value;
        var block = GameAPIs.BlockRegistry.Get(blockId);

        byte state = GetState(block, yaw);

        world.SetBlock(x, y, z, blockId, state);

        _placeCooldown = PlaceCooldown;
    }

    private static byte GetState(Block block, float yaw)
    {
        if (block.States?.Properties.TryGetValue("facing", out var facings) != true)
            return 0;

        string facing = GetFacing(yaw);

        for (byte i = 0; i < facings?.Count; i++)
        {
            if (facings[i] == facing)
                return i;
        }

        return 0;
    }

    private static string GetFacing(float yaw)
    {
        float angle = (yaw + MathF.PI) % (MathF.PI * 2f);

        if (angle < 0)
            angle += MathF.PI * 2f;

        return angle switch
        {
            < MathF.PI * 0.25f => "north",
            < MathF.PI * 0.75f => "west",
            < MathF.PI * 1.25f => "south",
            < MathF.PI * 1.75f => "east",
            _ => "north"
        };
    }

    private bool IsSolid(int x, int y, int z)
    {
        return world.GetBlock(x, y, z).Id != 0;
    }
}