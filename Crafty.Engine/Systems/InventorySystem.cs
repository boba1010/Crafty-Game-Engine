using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using System.Numerics;
using HudInventory = CraftyNative.HUD.Inventory;

namespace Crafty.Engine.Systems;

public sealed class InventorySystem : ISystem
{
    private bool _wasLeftDown;
    private bool _isOpen;
    private bool _needsFullSync;
    private Vector2 _cursor;
    private InventorySlot? _heldSlot;
    private bool _returnHeldOnClose;
    private static readonly Dictionary<ushort, CraftyNative.ThreeD.Meshes.Mesh> _meshCache = [];
    private readonly InventorySlot[] _synced = new InventorySlot[HudInventory.SlotCount];
    private const int MaxStack = 64;
    private int _shownTab = -1;
    private readonly int[] _tabPages = new int[HudInventory.MaxTabs];
    private bool _heldIsCreative;

    private static int PerPage => HudInventory.PaletteSlots;
    private static bool InPalette(int slot) => HudInventory.ActiveTab != 0 && slot >= HudInventory.Columns;

    private static ushort[] CategoryBlocks(int tab) => [1, 2, 3];      // e.g. BlockRegistry.ByCategory(tab - 1)
    private static ushort ItemIdFor(ushort blockId) => blockId;  // your real block -> item lookup

    public InventorySystem()
    {
        SystemAPI.Input.KeyDown += Input_KeyDown;
        SystemAPI.Input.MouseMove += Input_MouseMove;
    }

    private void Input_MouseMove(Vector2 position)
    {
        _cursor = position;

        if (GameStateManager.IsInventory)
            HudInventory.SetCursor(position);
    }

    private void Input_KeyDown(Key key)
    {
        if (GameStateManager.IsPaused)
            return;

        if (key == Key.E)
            if (_isOpen) CloseInventory(); else OpenInventory();
        else if (key == Key.Escape && GameStateManager.IsInventory)
            CloseInventory();
    }

    private void OpenInventory()
    {
        _isOpen = true;
        _needsFullSync = true;
        HudInventory.Open();
        HudInventory.SetCursor(_cursor);
        GameStateManager.Set(GameState.Inventory);
        SystemAPI.Input.CursorMode = Cursor.Normal;
    }

    private void CloseInventory()
    {
        _isOpen = false;
        _returnHeldOnClose = _heldSlot is not null;
        HudInventory.Close();
        GameStateManager.Set(GameState.Gameplay);
        SystemAPI.Input.CursorMode = Cursor.Raw;
    }

    public void Update(ref Scene scene, double deltaTime)
    {
        var player = scene.GetEntitiesWith<Inventory>().FirstOrDefault();
        if (!scene.HasComponent<Inventory>(player) || !scene.HasComponent<Hotbar>(player))
            return;

        ref var inventory = ref scene.GetComponent<Inventory>(player);
        ref var hotbar = ref scene.GetComponent<Hotbar>(player);
        ref var playerComponent = ref scene.GetComponent<Player>(player);

        if (_returnHeldOnClose)
        {
            ReturnHeldItem(ref inventory, ref hotbar);
            _returnHeldOnClose = false;
        }

        SyncChangedSlots(ref inventory, ref hotbar);

        bool down = SystemAPI.Input.IsMouseButtonDown(MouseButton.Left);
        bool justPressed = down && !_wasLeftDown;
        _wasLeftDown = down;

        if (!GameStateManager.IsInventory || GameStateManager.IsPaused)
            return;

        HudInventory.TabCount = playerComponent.GameMode is GameMode.Creative ? 2 : 0;

        if (_shownTab != HudInventory.ActiveTab)
            RefreshPalette();

        if (_needsFullSync)
        {
            SyncHeld();
            _needsFullSync = false;
        }

        if (justPressed)
        {
            int tab = HudInventory.HitTestTab(_cursor);

            if (tab != -1)
                HudInventory.SetActiveTab(tab);
            else
                HandleSlotClick(ref inventory, ref hotbar, HudInventory.HitTest(_cursor));
        }
    }

    private void RefreshPalette()
    {
        int tab = HudInventory.ActiveTab;
        var blocks = tab == 0 ? [] : CategoryBlocks(tab);

        for (int i = 0; i < PerPage; i++)
        {
            int idx = _tabPages[tab] * PerPage + i;

            if (idx < blocks.Length)
            {
                var s = new InventorySlot(ItemIdFor(blocks[idx]), 1, blocks[idx]);
                HudInventory.SetPaletteSlot(i, ResolveMesh(s), ResolveKey(s));
            }
            else
            {
                HudInventory.SetPaletteSlot(i, null);
            }
        }

        _shownTab = tab;
    }

    private void ChangePage(int delta)
    {
        int tab = HudInventory.ActiveTab;
        if (tab == 0) return;

        int pages = Math.Max(1, (CategoryBlocks(tab).Length + PerPage - 1) / PerPage);
        _tabPages[tab] = ((_tabPages[tab] + delta) % pages + pages) % pages;
        RefreshPalette();
    }

    private void HandlePaletteClick(int index)
    {
        if (_heldSlot is not null)
        {
            _heldSlot = null;
            _heldIsCreative = false;
            SyncHeld();
            return;
        }

        var blocks = CategoryBlocks(HudInventory.ActiveTab);
        int idx = _tabPages[HudInventory.ActiveTab] * PerPage + index;
        if (idx >= blocks.Length)
            return;

        _heldSlot = new InventorySlot(ItemIdFor(blocks[idx]), MaxStack, blocks[idx]);
        _heldIsCreative = true;
        SyncHeld();
    }

    private void HandleSlotClick(ref Inventory inventory, ref Hotbar hotbar, int slot)
    {
        if (slot == -1)
            return;

        if (InPalette(slot))
        {
            HandlePaletteClick(slot - HudInventory.Columns);
            return;
        }

        ref var clickedSlot = ref inventory.Slots[slot];

        if (_heldSlot is null)
        {
            if (clickedSlot.Count <= 0)
                return;

            _heldSlot = clickedSlot;
            clickedSlot = default;
        }
        else if (clickedSlot.Count <= 0)
        {
            clickedSlot = _heldSlot.Value;
            _heldSlot = null;
        }
        else
        {
            (clickedSlot, _heldSlot) = (_heldSlot.Value, clickedSlot);
        }

        _heldIsCreative = false;

        SyncSlot(ref inventory, ref hotbar, slot);
        SyncHeld();
    }

    private void ReturnHeldItem(ref Inventory inventory, ref Hotbar hotbar)
    {
        if (_heldIsCreative)
        {
            _heldSlot = null;
            _heldIsCreative = false;
            HudInventory.ClearHeldItem();
            return;
        }

        if (_heldSlot is not { } held)
            return;

        // Storage (9..35) first, then hotbar (0..8)
        for (int n = 0; n < HudInventory.SlotCount; n++)
        {
            int i = (n + HudInventory.Columns) % HudInventory.SlotCount;

            if (inventory.Slots[i].Count > 0)
                continue;

            inventory.Slots[i] = held;
            SyncSlot(ref inventory, ref hotbar, i);
            _heldSlot = null;
            HudInventory.ClearHeldItem();
            return;
        }

        // Inventory full: keep holding it; the full sync on reopen restores the cursor item
    }

    private static bool SameSlot(in InventorySlot a, in InventorySlot b)
        => a.Count == b.Count && a.ItemId == b.ItemId && a.BlockId == b.BlockId;

    private void SyncSlot(ref Inventory inventory, ref Hotbar hotbar, int slot)
    {
        var s = inventory.Slots[slot];
        _synced[slot] = s;

        if (slot < HudInventory.Columns)
            hotbar.Slots[slot] = s;

        var mesh = s.Count > 0 ? ResolveMesh(s) : null;
        var key = mesh is null ? null : ResolveKey(s);

        HudInventory.SetSlot(slot, mesh, key);

        if (slot < HudInventory.Columns)
            CraftyNative.HUD.Hotbar.SetSlot(slot, mesh, key);
    }

    private void SyncChangedSlots(ref Inventory inventory, ref Hotbar hotbar)
    {
        for (int i = 0; i < HudInventory.SlotCount; i++)
        {
            if (i < HudInventory.Columns && !SameSlot(hotbar.Slots[i], _synced[i]) && SameSlot(inventory.Slots[i], _synced[i]))
            {
                inventory.Slots[i] = hotbar.Slots[i];
            }

            if (!SameSlot(inventory.Slots[i], _synced[i]))
                SyncSlot(ref inventory, ref hotbar, i);
        }
    }

    private void SyncHeld()
    {
        if (_heldSlot is { } held && held.Count > 0)
            HudInventory.SetHeldItem(ResolveMesh(held), ResolveKey(held));
        else
            HudInventory.ClearHeldItem();
    }

    private static CraftyNative.ThreeD.Meshes.Mesh? ResolveMesh(InventorySlot slot)
    {
        if (slot.BlockId is not ushort id)
            return null;

        if (!_meshCache.TryGetValue(id, out var mesh))
            _meshCache[id] = mesh = BuildBlockMesh(id);

        return mesh;
    }

    private static string ResolveKey(InventorySlot slot)
        => slot.BlockId is ushort id ? $"block_{id}" : $"item_{slot.ItemId}";

    private static CraftyNative.ThreeD.Meshes.Mesh BuildBlockMesh(ushort blockId)
        => MeshBuilder.BuildBlockMesh(blockId);
}