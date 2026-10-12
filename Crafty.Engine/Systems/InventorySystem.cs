using Crafty.Engine.Components;
using Crafty.Engine.UI;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine.Systems;

public sealed class InventorySystem : IGameplaySystem
{
    // UI slot layout (same as HudInventory.SetSlot):
    // 0-8 hotbar, 9-35 storage, 36-39 armor, 40-43 crafting grid, 44 crafting result.
    private const int SlotCount = 45;

    /// <summary>Set once at startup: turns a stack into the mesh shown in the UI.</summary>
    public static Func<InventorySlot, Mesh?>? MeshResolver { get; set; }

    /// <summary>Set once at startup: maps an item id to its block id (null if the item isn't a block).</summary>
    public static Func<uint, uint?>? BlockIdResolver { get; set; }

    private readonly record struct UiChange(int Index, uint ItemId, int Count);

    private readonly Queue<UiChange> _pending = new();
    private readonly (uint ItemId, int Count)[] _synced = new (uint, int)[SlotCount];

    private bool _isOpen;

    public InventorySystem()
    {
        SystemAPI.Input.KeyDown += Input_KeyDown;
        HudInventory.SlotChanged += (index, itemId, count) => _pending.Enqueue(new(index, itemId, count));
    }

    private void Input_KeyDown(Key key)
    {
        if (GameStateManager.IsPaused)
            return;

        if (key == Key.E)
        {
            if (_isOpen) CloseInventory(); else OpenInventory();
        }
        else if (key == Key.Escape && GameStateManager.IsInventory)
        {
            CloseInventory();
        }
    }

    private void OpenInventory()
    {
        _isOpen = true;
        HudInventory.Open();
        GameStateManager.Set(GameState.Inventory);
        SystemAPI.Input.CursorMode = Cursor.Normal;
    }

    private void CloseInventory()
    {
        _isOpen = false;
        HudInventory.Close();
        GameStateManager.Set(GameState.Playing);
        SystemAPI.Input.CursorMode = Cursor.Raw;
    }

    // ------------------------------------------------------------ update

    public void Update(ref Scene scene, double deltaTime)
    {
        var player = scene.GetEntitiesWith<Inventory>().FirstOrDefault();
        if (!scene.HasComponent<Hotbar>(player))
            return;

        ref var inventory = ref scene.GetComponent<Inventory>(player);
        ref var hotbar = ref scene.GetComponent<Hotbar>(player);

        while (_pending.TryDequeue(out var change))
            ApplyUiChange(ref inventory, ref hotbar, change);

        PushToUi(ref inventory, ref hotbar);
    }

    private void ApplyUiChange(ref Inventory inventory, ref Hotbar hotbar, UiChange change)
    {
        ref var slot = ref SlotRef(ref inventory, ref hotbar, change.Index);

        if (change.Count > 0)
        {
            uint? blockId = slot.Count > 0 && slot.ItemId == change.ItemId ? slot.BlockId : BlockIdResolver?.Invoke(change.ItemId);
            slot = new InventorySlot(change.ItemId, change.Count, blockId);
        }
        else
        {
            slot = new InventorySlot(0, 0, null);
        }

        // Inventory.Slots[0..8] mirrors the hotbar.
        if (change.Index < 9)
            inventory.Slots[change.Index] = slot;

        _synced[change.Index] = (slot.ItemId, slot.Count);
    }

    private void PushToUi(ref Inventory inventory, ref Hotbar hotbar)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            ref var slot = ref SlotRef(ref inventory, ref hotbar, i);

            // The Hotbar component is the source of truth for slots 0-8; keep Inventory.Slots in step.
            if (i < 9)
                inventory.Slots[i] = slot;

            if (_synced[i] == (slot.ItemId, slot.Count))
                continue;

            _synced[i] = (slot.ItemId, slot.Count);

            Mesh? mesh = slot.Count > 0 ? MeshResolver?.Invoke(slot) : null;
            HudInventory.SetSlot(i, mesh, slot.ItemId, slot.Count);
        }
    }

    private static ref InventorySlot SlotRef(ref Inventory inventory, ref Hotbar hotbar, int index)
    {
        if (index < 9) return ref hotbar.Slots[index];
        if (index < 36) return ref inventory.Slots[index];
        if (index < 40) return ref inventory.Armor[index - 36];
        return ref inventory.Crafting[index - 40];
    }
}