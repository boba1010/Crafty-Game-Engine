using Crafty.Engine.Components;
using Crafty.Engine.Core;
using Crafty.Engine.UI;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine.Systems;

public sealed class InventorySystem : IGameplaySystem
{
    private const int SlotCount = 45;
    public static Func<InventorySlot, Mesh?>? MeshResolver { get; set; }
    public static Func<uint, uint?>? BlockIdResolver { get; set; }
    private readonly record struct UIChange(int Index, uint ItemId, int Count);
    private readonly Queue<UIChange> _pending = new();
    private readonly (uint ItemId, int Count)[] _synced = new (uint, int)[SlotCount];

    private static bool _isInserted;
    private static bool _isCreative;
    private static bool _isOpen;

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
            if (_isOpen) 
                CloseInventory(); 
            else 
                OpenInventory();
        }
        else if (key == Key.Escape && GameStateManager.IsInventory)
            CloseInventory();
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

    private static uint[] GetItemsIds() => GameAPIs.ItemRegistry.GetAllIds();

    // ------------------------------------------------------------ update

    public void Update(ref Scene scene, double deltaTime)
    {
        InsertItems();

        var playerObject = scene.GetEntitiesWith<Inventory>().FirstOrDefault();
        if (!scene.HasComponent<Hotbar>(playerObject))
            return;

        ref var inventory = ref scene.GetComponent<Inventory>(playerObject);
        ref var hotbar = ref scene.GetComponent<Hotbar>(playerObject);
        ref var player = ref scene.GetComponent<Player>(playerObject);

        if (player.GameMode is GameMode.Creative)
        {
            if (!_isCreative)
            {
                if (!_isInserted)
                    _isInserted = false;

                _isCreative = true;
            }
            HudInventory.SetCreativeMode(true);
        }
        else
        {
            _isCreative = false;
            HudInventory.SetCreativeMode(false);
        }

        while (_pending.TryDequeue(out var change))
            ApplyUIChange(ref inventory, ref hotbar, change);

        PushToUI(ref inventory, ref hotbar);
    }

    private void ApplyUIChange(ref Inventory inventory, ref Hotbar hotbar, UIChange change)
    {
        ref var slot = ref SlotRef(ref inventory, ref hotbar, change.Index);

        if (change.Count > 0)
        {
            uint? blockId = slot.Count > 0 && slot.ItemId == change.ItemId ? slot.BlockId : BlockIdResolver?.Invoke(change.ItemId);
            slot = new InventorySlot(change.ItemId, change.Count, blockId);
        }
        else
            slot = new InventorySlot(0, 0, null);

        // Inventory.Slots[0..8] mirrors the hotbar.
        if (change.Index < 9)
            inventory.Slots[change.Index] = slot;

        _synced[change.Index] = (slot.ItemId, slot.Count);
    }

    private void PushToUI(ref Inventory inventory, ref Hotbar hotbar)
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

    private static void InsertItems()
    {
        if (_isInserted)
            return;

        var items = GetItemsIds();

        for (int i = 0; i < items.Length; i++)
        {
            var itemId = items[i];
            var item = GameAPIs.ItemRegistry.Get(itemId);
            if (!string.IsNullOrEmpty(item.BlockId))
            {
                var blockId = GameAPIs.BlockRegistry.Get(item.BlockId);
                HudInventory.SetItem("Blocks", MeshBuilder.BuildBlockMesh(blockId), itemId);
            }
        }

        _isInserted = true;
    }
}