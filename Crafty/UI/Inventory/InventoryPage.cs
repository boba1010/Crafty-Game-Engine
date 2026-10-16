using CraftyNative.HUD;
using CraftyNative.UI;
using System.Numerics;

namespace Crafty.UI.Inventory;

public sealed class InventoryPage : Page, IInventoryLayout
{
    public SlotGrid Storage { get; } = new();

    public Grid InventoryModel { get; } = new()
    {
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new(0, 5, 0, 0),
        BackgroundColor = Vector4.Zero,
        Size = new(460, 200)
    };

    public SlotGrid ArmorSlots { get; } = new()
    {
        Columns = 1,
    };

    public ModelView PlayerModel { get; } = new()
    {
        Size = new(100, 200),
        Scale = new(1.5f, 1.5f, 1.5f)
    };

    public Grid CraftingSlotsWrapper { get; } = new()
    {
        BackgroundColor = Vector4.Zero,
        Size = new(250, 200)
    };

    public SlotGrid CraftingSlots { get; } = new()
    {
        Columns = 2,
    };

    public SlotGrid CraftingResultSlot { get; } = new()
    {
        Columns = 1,
    };

    public InventoryPage()
    {
        Initialize();
        BackgroundColor = Vector4.Zero;
        VerticalAlignment = VerticalAlignment.Bottom;
        HorizontalAlignment = HorizontalAlignment.Center;
    }

    private void Initialize()
    {
        var root = new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            BackgroundColor = Vector4.Zero,
        };

        root.Rows.Add(GridLength.Star);
        root.Rows.Add(new(160f, GridUnit.Pixel));
        root.Rows.Add(new(10f, GridUnit.Pixel));

        for (int i = 0; i < 27; i++)
        {
            Slot slot = new()
            {
                Size = new(Styles.SlotSize),
                IsMouseEnabled = true,
                IsHoverEnabled = true,
                CornerRadius = 0f,
                HoverBorderColor = Styles.Accent,
                HoverColor = new(1f, 1f, 1f, 0.1f)
            };
            slot.Clicked += OnSlotClick;

            Storage.Add(slot);
            SlotInteraction.Register(slot);
        }

        for (int i = 0; i < 4; i++)
        {
            Slot armorSlot = new()
            {
                Size = new(Styles.SlotSize),
                IsMouseEnabled = true,
                IsHoverEnabled = true,
                CornerRadius = 0f,
                HoverBorderColor = Styles.Accent,
                HoverColor = new(1f, 1f, 1f, 0.1f)
            };
            armorSlot.Clicked += OnSlotClick;
            ArmorSlots.Add(armorSlot);
            SlotInteraction.Register(armorSlot);

            Slot craftingSlot = new()
            {
                Size = new(Styles.SlotSize),
                IsMouseEnabled = true,
                IsHoverEnabled = true,
                CornerRadius = 0f,
                HoverBorderColor = Styles.Accent,
                HoverColor = new(1f, 1f, 1f, 0.1f)
            };
            craftingSlot.Clicked += OnSlotClick;
            CraftingSlots.Add(craftingSlot);
            SlotInteraction.Register(craftingSlot);
        }

        Slot resultSlot = new()
        {
            Size = new(Styles.SlotSize),
            IsMouseEnabled = true,
            IsHoverEnabled = true,
            CornerRadius = 0f,
            HoverBorderColor = Styles.Accent,
            HoverColor = new(1f, 1f, 1f, 0.1f)
        };
        resultSlot.Clicked += OnSlotClick;
        CraftingResultSlot.Add(resultSlot);
        SlotInteraction.RegisterOutput(resultSlot);

        PlayerModel.Model = MeshBuilder.BuildPlayerMesh();

        InventoryModel.Columns.Add(new(60, GridUnit.Pixel));
        InventoryModel.Columns.Add(new(100, GridUnit.Pixel));
        InventoryModel.Columns.Add(new(300, GridUnit.Pixel));

        CraftingSlotsWrapper.Columns.Add(new(200, GridUnit.Pixel));
        CraftingSlotsWrapper.Columns.Add(new(50, GridUnit.Pixel));

        CraftingSlotsWrapper.Add(CraftingSlots, 0, 0);
        CraftingSlotsWrapper.Add(CraftingResultSlot, 0, 1);

        InventoryModel.Add(ArmorSlots, 0, 0);
        InventoryModel.Add(PlayerModel, 0, 1);
        InventoryModel.Add(CraftingSlotsWrapper, 0, 2);

        root.Add(InventoryModel, 0);
        root.Add(Storage, 1);
        root.Add(new Seperator()
        {
            Thickness = 2f,
            Color = Styles.Accent,
            VerticalAlignment = VerticalAlignment.Top,
            Size = new(400, 2),
        }, 2);

        Content = root;
    }

    private void OnSlotClick(Slot sender) => SlotInteraction.OnClick(sender);
}