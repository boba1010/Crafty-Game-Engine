using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using System.Numerics;
using HudInventory = CraftyNative.HUD.Inventory;

namespace Crafty.Engine.Systems;

public sealed class InventorySystem : ISystem
{
    private const int MaxStack = 64;

    private bool _wasLeftDown;
    private bool _isOpen;
    private bool _needsFullSync;
    private bool _returnHeldOnClose;
    private bool _heldIsCreative;
    private Vector2 _cursor;
    private InventorySlot? _heldSlot;

    private int _shownTab = -1;
    private readonly int[] _tabPages = new int[HudInventory.MaxTabs];

    private static readonly Dictionary<ushort, CraftyNative.ThreeD.Meshes.Mesh> _meshCache = [];
    private readonly InventorySlot[] _synced = new InventorySlot[HudInventory.TotalSlots];
    private bool _modelSynced;

    private static int PerPage => HudInventory.PaletteSlots;
    private static bool InPalette(int slot) => HudInventory.ActiveTab != 0 && slot >= HudInventory.Columns;

    // ---- Placeholders: replace with your registries ----
    private static ushort[] CategoryBlocks(int tab) => [1, 2, 3];
    private static ushort ItemIdFor(ushort blockId) => blockId;
    private static InventorySlot MatchRecipe(ReadOnlySpan<InventorySlot> grid2x2) => default;
    private static bool CanPlace(int slot, in InventorySlot item) => true;

    public InventorySystem()
    {
        SystemAPI.Input.KeyDown += Input_KeyDown;
        SystemAPI.Input.MouseMove += Input_MouseMove;
    }

    // ------------------------------------------------------------ input events

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
        _needsFullSync = true;
        HudInventory.Open();
        HudInventory.SetCursor(_cursor);
        GameStateManager.Set(GameState.Inventory);
        SystemAPI.Input.CursorMode = Cursor.Normal;
    }

    private void CloseInventory()
    {
        _isOpen = false;
        _returnHeldOnClose = true;   // also returns the crafting grid, so always set
        HudInventory.Close();
        GameStateManager.Set(GameState.Playing);
        SystemAPI.Input.CursorMode = Cursor.Raw;
    }

    // ------------------------------------------------------------ update

    public void Update(ref Scene scene, double deltaTime)
    {
        var player = scene.GetEntitiesWith<Inventory>().FirstOrDefault();
        if (!scene.HasComponent<Inventory>(player) || !scene.HasComponent<Hotbar>(player) || !scene.HasComponent<Player>(player))
            return;

        ref var inventory = ref scene.GetComponent<Inventory>(player);
        ref var hotbar = ref scene.GetComponent<Hotbar>(player);
        ref var playerComponent = ref scene.GetComponent<Player>(player);

        if (_returnHeldOnClose)
        {
            ReturnHeldItem(ref inventory, ref hotbar);
            ReturnCraftingGrid(ref inventory, ref hotbar);
            _returnHeldOnClose = false;
        }

        SyncChangedSlots(ref inventory, ref hotbar);   // runs in every game state

        // Edge detection runs in every state so a held button doesn't "click" when the inventory opens
        bool down = SystemAPI.Input.IsMouseButtonDown(MouseButton.Left);
        bool justPressed = down && !_wasLeftDown;
        _wasLeftDown = down;

        if (!GameStateManager.IsInventory || GameStateManager.IsPaused)
            return;

        if (!_modelSynced)
        {
            var model = new WorldObject(1);

            ref var renderable = ref scene.GetRenderable(model);
            HudInventory.SetPlayerModel(renderable.Mesh, "player_model");

            _modelSynced = true;
        }

        HudInventory.TabCount = playerComponent.GameMode is GameMode.Creative ? 2 : 0;
        HudInventory.CraftingVisible = playerComponent.GameMode is not GameMode.Creative;

        if (_shownTab != HudInventory.ActiveTab)
            RefreshPalette();

        if (_needsFullSync)
        {
            SyncHeld();
            _needsFullSync = false;
        }

        if (!justPressed)
            return;

        int tab = HudInventory.HitTestTab(_cursor);
        int page = HudInventory.HitTestPage(_cursor);

        if (tab != -1)
        {
            HudInventory.SetActiveTab(tab);

            if (_shownTab != HudInventory.ActiveTab)
                RefreshPalette();
        }
        else if (page != -1)
        {
            ChangePage(page == 0 ? -1 : 1);
        }
        else
        {
            HandleSlotClick(ref inventory, ref hotbar, HudInventory.HitTest(_cursor));
        }
    }

    // ------------------------------------------------------------ creative palette

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
        if (_heldSlot is not null)            // clicking the palette while holding = delete
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

    // ------------------------------------------------------------ clicks

    private void HandleSlotClick(ref Inventory inventory, ref Hotbar hotbar, int slot)
    {
        if (slot == -1)
            return;

        if (InPalette(slot))
        {
            HandlePaletteClick(slot - HudInventory.Columns);
            return;
        }

        if (slot == HudInventory.OutputSlot)
        {
            HandleOutputClick(ref inventory, ref hotbar);
            return;
        }

        if (_heldSlot is { } incoming && !CanPlace(slot, incoming))
            return;

        ref var clickedSlot = ref SlotAt(ref inventory, slot);

        if (_heldSlot is null)
        {
            if (clickedSlot.Count <= 0)
                return;

            _heldSlot = clickedSlot;
            clickedSlot = default;
            _heldIsCreative = false;
        }
        else if (clickedSlot.Count <= 0)
        {
            clickedSlot = _heldSlot.Value;
            _heldSlot = null;
            _heldIsCreative = false;
        }
        else if (SameItem(clickedSlot, _heldSlot.Value))
        {
            // Merge: fill the clicked stack, keep the remainder on the cursor
            var held = _heldSlot.Value;
            int move = Math.Min(MaxStackFor(clickedSlot) - clickedSlot.Count, held.Count);

            if (move <= 0)
                return;   // clicked stack is already full

            clickedSlot.Count += move;
            held.Count -= move;

            if (held.Count > 0)
            {
                _heldSlot = held;
            }
            else
            {
                _heldSlot = null;
                _heldIsCreative = false;
            }
        }
        else
        {
            (clickedSlot, _heldSlot) = (_heldSlot.Value, clickedSlot);
            _heldIsCreative = false;
        }

        SyncSlot(ref inventory, ref hotbar, slot);

        if (slot >= HudInventory.CraftStart && slot < HudInventory.OutputSlot)
            UpdateCraftOutput(ref inventory, ref hotbar);

        SyncHeld();
    }

    // ------------------------------------------------------------ crafting

    private void UpdateCraftOutput(ref Inventory inventory, ref Hotbar hotbar)
    {
        ref var c = ref inventory.Crafting;
        ReadOnlySpan<InventorySlot> grid = [c[0], c[1], c[2], c[3]];

        inventory.Crafting[4] = MatchRecipe(grid);
        SyncSlot(ref inventory, ref hotbar, HudInventory.OutputSlot);
    }

    private void HandleOutputClick(ref Inventory inventory, ref Hotbar hotbar)
    {
        ref var output = ref inventory.Crafting[4];
        if (output.Count <= 0 || _heldSlot is not null)
            return;

        _heldSlot = output;
        _heldIsCreative = false;

        for (int i = 0; i < 4; i++)           // consumes one of each ingredient
        {
            ref var s = ref inventory.Crafting[i];
            if (s.Count <= 0) continue;

            if (--s.Count <= 0) s = default;
            SyncSlot(ref inventory, ref hotbar, HudInventory.CraftStart + i);
        }

        UpdateCraftOutput(ref inventory, ref hotbar);
        SyncHeld();
    }

    private void ReturnCraftingGrid(ref Inventory inventory, ref Hotbar hotbar)
    {
        for (int c = 0; c < 4; c++)
        {
            var item = inventory.Crafting[c];
            if (item.Count <= 0) continue;

            // Storage (9..35) first, then hotbar (0..8)
            for (int n = 0; n < HudInventory.SlotCount; n++)
            {
                int i = (n + HudInventory.Columns) % HudInventory.SlotCount;
                if (inventory.Slots[i].Count > 0) continue;

                inventory.Slots[i] = item;
                inventory.Crafting[c] = default;
                SyncSlot(ref inventory, ref hotbar, i);
                SyncSlot(ref inventory, ref hotbar, HudInventory.CraftStart + c);
                break;
            }
        }

        UpdateCraftOutput(ref inventory, ref hotbar);
    }

    // ------------------------------------------------------------ held item

    private void ReturnHeldItem(ref Inventory inventory, ref Hotbar hotbar)
    {
        if (_heldIsCreative)                  // palette items are discarded, not stored
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

    // ------------------------------------------------------------ ECS <-> HUD sync

    private static ref InventorySlot SlotAt(ref Inventory inv, int slot)
    {
        if (slot < HudInventory.SlotCount) return ref inv.Slots[slot];
        if (slot < HudInventory.CraftStart) return ref inv.Armor[slot - HudInventory.ArmorStart];
        return ref inv.Crafting[slot - HudInventory.CraftStart];
    }

    private static bool SameSlot(in InventorySlot a, in InventorySlot b)
        => a.Count == b.Count && a.ItemId == b.ItemId && a.BlockId == b.BlockId;

    private void SyncSlot(ref Inventory inventory, ref Hotbar hotbar, int slot)
    {
        var s = SlotAt(ref inventory, slot);
        _synced[slot] = s;

        if (slot < HudInventory.Columns)
            hotbar.Slots[slot] = s;   // keep the ECS hotbar component in step

        var mesh = s.Count > 0 ? ResolveMesh(s) : null;
        var key = mesh is null ? null : ResolveKey(s);

        HudInventory.SetSlot(slot, mesh, key, s.Count);

        if (slot < HudInventory.Columns)
            CraftyNative.HUD.Hotbar.SetSlot(slot, mesh, key, s.Count);
    }

    private void SyncChangedSlots(ref Inventory inventory, ref Hotbar hotbar)
    {
        for (int i = 0; i < HudInventory.TotalSlots; i++)
        {
            // Hotbar component changed (e.g. a block was placed) and the inventory didn't:
            // pull the change into the inventory, then fall through to push it to the HUD.
            if (i < HudInventory.Columns && !SameSlot(hotbar.Slots[i], _synced[i]) && SameSlot(inventory.Slots[i], _synced[i]))
            {
                inventory.Slots[i] = hotbar.Slots[i];
            }

            if (!SameSlot(SlotAt(ref inventory, i), _synced[i]))
                SyncSlot(ref inventory, ref hotbar, i);
        }
    }

    private void SyncHeld()
    {
        if (_heldSlot is { } held && held.Count > 0)
            HudInventory.SetHeldItem(ResolveMesh(held), ResolveKey(held), held.Count);
        else
            HudInventory.ClearHeldItem();
    }

    // ------------------------------------------------------------ mesh lookup

    private static CraftyNative.ThreeD.Meshes.Mesh? ResolveMesh(InventorySlot slot)
    {
        if (slot.BlockId is not ushort id)
            return null; // non-block item: needs a flat icon path later

        if (!_meshCache.TryGetValue(id, out var mesh))
            _meshCache[id] = mesh = BuildBlockMesh(id);

        return mesh;
    }

    private static string ResolveKey(InventorySlot slot) 
        => slot.BlockId is ushort id ? $"block_{id}" : $"item_{slot.ItemId}";

    private static CraftyNative.ThreeD.Meshes.Mesh BuildBlockMesh(ushort blockId)
        => MeshBuilder.BuildBlockMesh(blockId);

    private static int MaxStackFor(in InventorySlot s) => MaxStack;

    private static bool SameItem(in InventorySlot a, in InventorySlot b)
        => a.ItemId == b.ItemId && a.BlockId == b.BlockId;
}