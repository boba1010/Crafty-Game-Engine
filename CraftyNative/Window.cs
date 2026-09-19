using CraftyNative.ThreeD;
using CraftyNative.TwoD;
using CraftyNative.TwoD.Animations;
using CraftyNative.TwoD.Controls;
using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Numerics;

namespace CraftyNative;

public abstract class Window
{
    public GraphicsMode GraphicsMode { get; set; }

    public UIElement Root { get; set; } = null!;

    private string _title = "Window";
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            _window?.Title = value;
        }
    }

    public event EventHandler? Activated;

    private IInputContext _input = null!;
    private IWindow _window = null!;

    public void Initialize()
    {
        var options = WindowOptions.Default;
        options.Title = "Window";
        options.API = GraphicsAPI.None;
        options.Size = new(1280, 720);
        options.VSync = false;

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += OnWindowLoad;
        _window.Render += OnWindowRender;
        _window.Resize += OnWindowResize;
    }

    private void OnWindowLoad()
    {
        _input = _window.CreateInput();
        foreach (var mouse in _input.Mice)
        {
            mouse.MouseDown += OnMouseDown;
            mouse.MouseMove += OnMouseMove;
            mouse.MouseUp += OnMouseUp;
        }
        CraftyNative2D.Initialize(_window);
        CraftyNative3D.Initialize(_window);

        Activated?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowRender(double deltaTime)
    {
        Render(deltaTime);
    }

    /// <summary>
    /// Override for 3D or custom rendering
    /// </summary>
    /// <param name="deltaTime"></param>
    public virtual void Render(double deltaTime)
    {
        CraftyNative2D.Device.BeginDraw();

        foreach (var animation in AnimationsManager.Animations)
            animation.Tick(deltaTime);

        Root?.Render(deltaTime);

        CraftyNative2D.Device.EndDraw();
    }

    private void OnWindowResize(Silk.NET.Maths.Vector2D<int> obj)
    {
        ResizeDpiScale(obj.X, obj.Y);
    }

    public void Activate()
    {
        _window.Run();
    }

    private void ResizeDpiScale(int width, int height)
    {
        if (GraphicsMode is GraphicsMode.TwoD or GraphicsMode.TwoDAndThreeD)
        {
            CraftyNative2D.ResizeFrameBuffer((uint)width, (uint)height);
            CraftyNative2D.UpdateDpi();
            Root.Layout(new(0, 0, CraftyNative2D.Height, CraftyNative2D.Width));
        }

        if (GraphicsMode is GraphicsMode.ThreeD or GraphicsMode.TwoDAndThreeD)
        {
            // 3D
        }
    }

    private UIElement? _hoveredElement;
    private UIElement? _pressedElement;

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (button != MouseButton.Left)
            return;

        _pressedElement = _hoveredElement;
        _pressedElement?.RaisePressed();
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        if (button != MouseButton.Left)
            return;

        _pressedElement?.RaiseReleased();
        _pressedElement = null;
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        var x = position.X / CraftyNative2D.DpiScaleX;
        var y = position.Y / CraftyNative2D.DpiScaleY;

        var element = Root.FindHit(x, y);

        if (element == _hoveredElement)
            return;

        _hoveredElement?.RaiseExit();

        _hoveredElement = element;

        _hoveredElement?.RaiseEnter();
    }

    public void Dispose()
    {
        foreach (var mouse in _input.Mice)
        {
            mouse.MouseDown -= OnMouseDown;
            mouse.MouseMove -= OnMouseMove;
            mouse.MouseUp -= OnMouseUp;
        }

        CraftyNative2D.Dispose();
        CraftyNative3D.Dispose();
    }
}
