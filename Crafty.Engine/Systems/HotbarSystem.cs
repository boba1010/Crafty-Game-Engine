using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine.Systems;

public sealed class HotbarSystem : ISystem
{
    private const int SlotCount = 9;
    private int _selectedSlot;
    private readonly ushort?[] _shownBlocks = new ushort?[SlotCount];
    private readonly Dictionary<int, Mesh> _blockMeshes = [];

    public HotbarSystem()
    {
        SystemAPI.Input.KeyDown += Input_KeyDown;
    }

    private void Input_KeyDown(Key key)
    {
        int slot = key switch
        {
            Key.Number1 => 0,
            Key.Number2 => 1,
            Key.Number3 => 2,
            Key.Number4 => 3,
            Key.Number5 => 4,
            Key.Number6 => 5,
            Key.Number7 => 6,
            Key.Number8 => 7,
            Key.Number9 => 8,
            _ => -1
        };

        if (slot < 0 || slot == _selectedSlot)
            return;

        _selectedSlot = slot;
        CraftyNative.HUD.Hotbar.SetSelectedSlot(slot);
    }

    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var entity in scene.GetEntitiesWith<Hotbar>())
        {
            ref var hotbar = ref scene.GetComponent<Hotbar>(entity);
            hotbar.SelectedSlot = _selectedSlot;

            for (int i = 0; i < SlotCount; i++)
            {
                ushort? blockId = hotbar.Slots[i].BlockId is { } id ? (ushort)id : null;

                if (blockId == _shownBlocks[i])
                    continue;

                _shownBlocks[i] = blockId;

                if (blockId is ushort block)
                    CraftyNative.HUD.Hotbar.SetSlot(i, GetBlockMesh(block), $"hotbar_block_{block}");
                else
                    CraftyNative.HUD.Hotbar.SetSlot(i, null);
            }
        }
    }

    private Mesh GetBlockMesh(ushort blockId)
    {
        if (_blockMeshes.TryGetValue(blockId, out var cached))
            return cached;

        var mesh = MeshBuilder.BuildBlockMesh(blockId);
        _blockMeshes[blockId] = mesh;
        return mesh;
    }
}