using CraftyNative;
using CraftyNative.HUD;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine.UI.Inventory;

public static class SlotInteraction
{
    public const int MaxStack = 64;

    private static readonly Dictionary<Slot, (float X, float Y)> _rotations = [];
    private static readonly HashSet<Slot> _outputOnly = [];

    public static Mesh? HeldMesh { get; private set; }
    public static uint HeldId { get; private set; }
    public static int HeldCount { get; private set; }
    public static bool IsHolding => HeldCount > 0;

    public static void Register(Slot slot, float rotationX = MathF.PI / 6, float rotationY = MathF.PI / 4)
        => _rotations[slot] = (rotationX, rotationY);

    public static void RegisterOutput(Slot slot)
    {
        Register(slot);
        _outputOnly.Add(slot);
    }

    public static void Apply(Slot slot, Mesh? mesh, uint itemId, int count)
    {
        if (count <= 0)
        {
            slot.Content = null;
            slot.Tag = 0u;
            slot.Count = 0;
            SlotChanged?.Invoke(slot);
            return;
        }

        var (rx, ry) = _rotations.TryGetValue(slot, out var r) ? r : (MathF.PI / 6, MathF.PI / 4);
        slot.Content = new()
        {
            Model = mesh,
            Rotation = new(rx, ry, 0f)
        };
        slot.Tag = itemId;
        slot.Count = count;
        SlotChanged?.Invoke(slot);
    }

    public static event Action<Slot>? SlotChanged;

    public static void OnClick(Slot slot) => Click(slot, SystemAPI.Input.IsMouseButtonDown(MouseButton.Right));

    public static void OnCreativeClick(Slot slot)
    {
        if (slot.Count <= 0)
        {
            SetHeld(null, 0, 0);
            return;
        }

        bool right = SystemAPI.Input.IsMouseButtonDown(MouseButton.Right);
        uint id = IdOf(slot);
        Mesh? mesh = slot.Content?.Model;
        int amount = right ? 1 : MaxStack;

        if (IsHolding && HeldId == id)
            SetHeld(mesh, id, Math.Min(HeldCount + amount, MaxStack));
        else
            SetHeld(mesh, id, amount);
    }

    public static void Click(Slot slot, bool right)
    {
        if (_outputOnly.Contains(slot))
        {
            TakeOutput(slot);
            return;
        }

        int slotCount = Math.Max(slot.Count, 0);
        uint slotId = IdOf(slot);
        Mesh? slotMesh = slot.Content?.Model;

        if (!IsHolding)
        {
            if (slotCount == 0)
                return;

            int take = right ? (slotCount + 1) / 2 : slotCount;
            SetHeld(slotMesh, slotId, take);
            Apply(slot, slotMesh, slotId, slotCount - take);
            return;
        }

        if (slotCount == 0)
        {
            int put = right ? 1 : HeldCount;
            Apply(slot, HeldMesh, HeldId, put);
            SetHeld(HeldMesh, HeldId, HeldCount - put);
            return;
        }

        if (slotId == HeldId)
        {
            int space = MaxStack - slotCount;
            int move = Math.Min(right ? 1 : HeldCount, space);
            if (move <= 0)
                return;

            Apply(slot, slotMesh, slotId, slotCount + move);
            SetHeld(HeldMesh, HeldId, HeldCount - move);
            return;
        }

        Mesh? heldMesh = HeldMesh;
        uint heldId = HeldId;
        int heldCount = HeldCount;

        SetHeld(slotMesh, slotId, slotCount);
        Apply(slot, heldMesh, heldId, heldCount);
    }

    private static void TakeOutput(Slot slot)
    {
        int count = Math.Max(slot.Count, 0);
        if (count == 0)
            return;

        uint id = IdOf(slot);
        Mesh? mesh = slot.Content?.Model;

        if (!IsHolding)
        {
            SetHeld(mesh, id, count);
            Apply(slot, null, 0, 0);
        }
        else if (HeldId == id && HeldCount + count <= MaxStack)
        {
            SetHeld(HeldMesh, HeldId, HeldCount + count);
            Apply(slot, null, 0, 0);
        }
    }

    public static void ReturnHeld(IEnumerable<Slot> slots)
    {
        if (!IsHolding)
            return;

        var list = slots.ToList();

        foreach (var slot in list)
        {
            if (!IsHolding)
                break;

            if (slot.Count > 0 && IdOf(slot) == HeldId && slot.Count < MaxStack)
            {
                int move = Math.Min(MaxStack - slot.Count, HeldCount);
                Apply(slot, slot.Content?.Model, HeldId, slot.Count + move);
                SetHeld(HeldMesh, HeldId, HeldCount - move);
            }
        }

        foreach (var slot in list)
        {
            if (!IsHolding)
                break;

            if (slot.Count <= 0)
            {
                int move = Math.Min(MaxStack, HeldCount);
                Apply(slot, HeldMesh, HeldId, move);
                SetHeld(HeldMesh, HeldId, HeldCount - move);
            }
        }
    }

    private static uint IdOf(Slot slot) => slot.Tag is uint id ? id : 0u;

    private static void SetHeld(Mesh? mesh, uint id, int count)
    {
        if (count <= 0)
        {
            HeldMesh = null;
            HeldId = 0;
            HeldCount = 0;
            return;
        }

        HeldMesh = mesh;
        HeldId = id;
        HeldCount = count;
    }
}