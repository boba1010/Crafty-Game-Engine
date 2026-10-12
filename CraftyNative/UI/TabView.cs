using System.Diagnostics;
using System.Numerics;

namespace CraftyNative.UI;

public sealed class TabView : UIElement
{
    private readonly List<TabItem> _tabs = [];
    private bool _wasMousePressed;
    private readonly Stopwatch _hoverTimer = new();
    private int _tooltipIndex = -1;

    private const double TooltipDelayMs = 500;
    private const float TooltipPadding = 8f;
    private const float TooltipOffset = 12f;

    public Vector4 TooltipBackground { get; set; } = new(0.08f, 0.08f, 0.08f, 0.98f);
    public Vector4 TooltipForeground { get; set; } = Vector4.One;
    public float TooltipFontScale { get; set; } = 0.8f;

    public IReadOnlyList<TabItem> Tabs => _tabs;
    public int SelectedIndex { get; private set; } = -1;
    public TabItem? SelectedTab => (uint)SelectedIndex < (uint)_tabs.Count ? _tabs[SelectedIndex] : null;
    public float HeaderHeight { get; set; } = 40f;
    public float TabWidth { get; set; } = 50f;
    public int HoveredIndex { get; private set; } = -1;
    public Vector4 SelectedTabColor { get; set; } = new(0.20f, 0.20f, 0.20f, 1f);
    public Vector4 HoveredTabColor { get; set; } = new(0.12f, 0.12f, 0.12f, 1f);
    public Vector4 TabColor { get; set; } = new(0f, 0f, 0f, 1f);
    public event Action<TabItem?>? SelectionChanged;
    public Vector4 Background { get; set; } = Vector4.One;
    private UIElement? _content;

    public UIElement? Content
    {
        get => _content;
        set
        {
            if (ReferenceEquals(_content, value))
                return;

            if (_content?.Parent == this)
                _content.Parent = null;

            _content = value;

            if (_content is not null)
            {
                _content.Parent = this;
                ArrangeContent();
            }
        }
    }

    public void AddTab(TabItem tab)
    {
        ArgumentNullException.ThrowIfNull(tab);

        if (_tabs.Contains(tab))
            return;

        _tabs.Add(tab);

        if (SelectedIndex < 0)
            SelectTab(0);
    }

    public bool RemoveTab(TabItem tab)
    {
        int index = _tabs.IndexOf(tab);

        if (index < 0)
            return false;

        _tabs.RemoveAt(index);

        if (_tabs.Count == 0)
        {
            SelectedIndex = -1;
            SelectionChanged?.Invoke(null);
        }
        else if (index == SelectedIndex)
        {
            SelectedIndex = -1;
            SelectTab(Math.Min(index, _tabs.Count - 1));
        }
        else if (index < SelectedIndex)
        {
            SelectedIndex--;
        }

        return true;
    }

    public bool SelectTab(int index)
    {
        if ((uint)index >= (uint)_tabs.Count || index == SelectedIndex)
            return false;

        SelectedIndex = index;
        ArrangeContent();
        SelectionChanged?.Invoke(SelectedTab);

        return true;
    }

    private void ArrangeContent()
    {
        if (Content is not { } element)
            return;

        element.Position = new Vector2(0, HeaderHeight);
        element.Arrange(new Vector2(ActualSize.X, Math.Max(0, ActualSize.Y - HeaderHeight)));
    }


    public override Vector2 Measure(Vector2 availableSize)
    {
        AvailableSize = Vector2.Max(availableSize, Vector2.Zero);

        Vector2 desiredSize = Vector2.Min(Vector2.Max(Size, Vector2.Zero), AvailableSize);

        return desiredSize + new Vector2(Margin.Left + Margin.Right, Margin.Top + Margin.Bottom);
    }

    public override void Arrange(Vector2 finalSize)
    {
        Vector2 availableSize = Vector2.Max(
            finalSize - new Vector2(Margin.Left + Margin.Right, Margin.Top + Margin.Bottom), Vector2.Zero);

        ActualSize = Vector2.Min(Vector2.Max(Size, Vector2.Zero), availableSize);

        float x = HorizontalAlignment switch
        {
            HorizontalAlignment.Left => Margin.Left,
            HorizontalAlignment.Center => Margin.Left + (availableSize.X - ActualSize.X) / 2f,
            HorizontalAlignment.Right => finalSize.X - Margin.Right - ActualSize.X,
            HorizontalAlignment.Stretch => Margin.Left,
            _ => Margin.Left
        };

        float y = VerticalAlignment switch
        {
            VerticalAlignment.Top => Margin.Top,
            VerticalAlignment.Center => Margin.Top + (availableSize.Y - ActualSize.Y) / 2f,
            VerticalAlignment.Bottom => finalSize.Y - Margin.Bottom - ActualSize.Y,
            VerticalAlignment.Stretch => Margin.Top,
            _ => Margin.Top
        };

        Position = new Vector2(x, y);
        ActualSize = new Vector2(
            HorizontalAlignment == HorizontalAlignment.Stretch ? availableSize.X : ActualSize.X,
            VerticalAlignment == VerticalAlignment.Stretch ? availableSize.Y : ActualSize.Y);

        ArrangeContent();
    }


    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        if (!IsVisible)
        {
            ResetTooltip();
            _wasMousePressed = mousePressed;
            HoveredIndex = -1;
            return;
        }

        Vector2 position = AbsolutePosition;
        bool justPressed = mousePressed && !_wasMousePressed;
        _wasMousePressed = mousePressed;

        HoveredIndex = -1;

        bool inHeader =
            cursorPosition.X >= position.X &&
            cursorPosition.X < position.X + ActualSize.X &&
            cursorPosition.Y >= position.Y &&
            cursorPosition.Y < position.Y + HeaderHeight;

        if (inHeader && TabWidth > 0)
        {
            int index = (int)((cursorPosition.X - position.X) / TabWidth);

            if ((uint)index < (uint)_tabs.Count)
            {
                HoveredIndex = index;

                if (justPressed)
                    SelectTab(index);
            }
        }

        if (HoveredIndex != _tooltipIndex)
        {
            _tooltipIndex = HoveredIndex;
            _hoverTimer.Reset();

            if (_tooltipIndex >= 0 && !string.IsNullOrWhiteSpace(_tabs[_tooltipIndex].TooltipDescription))
                _hoverTimer.Start();
        }
        else if (HoveredIndex < 0)
            ResetTooltip();

        if (Content is { IsVisible: true } element)
            element.Update(cursorPosition, windowSize, mousePressed);
    }

    private void ResetTooltip()
    {
        _tooltipIndex = -1;
        _hoverTimer.Reset();
    }

    private void DrawTooltip()
    {
        if (HoveredIndex < 0 || 
            HoveredIndex != _tooltipIndex || 
            !_hoverTimer.IsRunning || 
            _hoverTimer.Elapsed.TotalMilliseconds < TooltipDelayMs)
        {
            return;
        }

        string description = _tabs[_tooltipIndex].TooltipDescription!;

        if (string.IsNullOrWhiteSpace(description))
            return;

        Vector2 mousePosition = InputManager.MousePosition;
        Vector2 textSize = UITextRenderer.MeasureText(description, TooltipFontScale);

        Vector2 tooltipSize = textSize + new Vector2(TooltipPadding * 2f, TooltipPadding * 2f);

        Vector2 tooltipPosition = mousePosition + new Vector2(TooltipOffset, TooltipOffset);

        if (tooltipPosition.X + tooltipSize.X > SystemAPI.WindowSize.X)
            tooltipPosition.X = mousePosition.X - tooltipSize.X - TooltipOffset;

        if (tooltipPosition.Y + tooltipSize.Y > SystemAPI.WindowSize.Y)
            tooltipPosition.Y = mousePosition.Y - tooltipSize.Y - TooltipOffset;

        UIRenderer.DrawOverlayRectangle(tooltipPosition, tooltipSize, TooltipBackground);

        UITextRenderer.DrawOverlayTextCentered(description, tooltipPosition, tooltipSize, TooltipFontScale, TooltipForeground);
    }

    public void DrawOverlay()
    {
        if (!IsVisible)
            return;

        DrawTooltip();
    }

    public override void Draw()
    {
        if (!IsVisible)
            return;

        Vector2 position = AbsolutePosition;

        UIRenderer.DrawRectangle(position, Size, Background);

        UIRenderer.PushClip(position, new Vector2(ActualSize.X, HeaderHeight));

        try
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                Vector2 tabPosition = position + new Vector2(i * TabWidth, 0);

                Vector4 color = i == SelectedIndex ? new Vector4(0.20f, 0.20f, 0.20f, 1f) : TabColor;

                UIRenderer.DrawRectangle(tabPosition, new Vector2(TabWidth, HeaderHeight), color);

                if (_tabs[i].Icon is { } mesh)
                    UIRenderer.DrawMesh(mesh, tabPosition, new(MathF.PI / 6, MathF.PI / 4, 0f), new(0.8f, 0.8f, 0.8f), new(TabWidth, HeaderHeight));
                else
                    UITextRenderer.DrawTextCentered(_tabs[i].Header, tabPosition, new Vector2(TabWidth, HeaderHeight), 0.8f, Vector4.One);
            }
        }
        finally
        {
            UIRenderer.PopClip();
        }

        if (Content is { IsVisible: true } element)
            element?.Draw();
        DrawTooltip();
    }
}