using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Numerics;

namespace CraftyNative;

public abstract class Window : IDisposable
{
    private string _title = "Window";
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            NativeWindow?.Title = value;
        }
    }

    public event EventHandler? Activated;
    public event EventHandler<bool>? Focused;
    public InputManager InputManager { get; private set; } = null!;
    public IWindow NativeWindow { get; private set; } = null!;
    public Vector2D<int> Size { get; private set; } = new(1280, 720);

    public void Initialize()
    {
        var options = WindowOptions.Default;
        options.Title = "Window";
        options.API = GraphicsAPI.None;
        options.Size = new(1280, 720);
        options.VSync = true;

        NativeWindow = Silk.NET.Windowing.Window.Create(options);

        NativeWindow.Load += OnWindowLoad;
        NativeWindow.Render += OnWindowRender;
        NativeWindow.Resize += OnWindowResize;
        NativeWindow.FocusChanged += NativeWindow_FocusChanged;
    }

    private void NativeWindow_FocusChanged(bool focused)
    {
        Focused?.Invoke(this, focused);
    }

    public void Resize(Vector2D<int> size)
    {
        Size = size;
        NativeWindow.Size = size;
        OnResize(size);
    }


    protected virtual void OnResize(Vector2D<int> size) { }
    protected virtual void OnMouseMove(Vector2 position) { }
    protected virtual void OnKeyDown(Key key) { }
    protected virtual void OnKeyUp(Key key) { }
    protected virtual void OnMouseClick(MouseButton button, Vector2 position) { }
    protected virtual void Render(double deltaTime) { }

    private void OnWindowLoad()
    {
        InputManager = new(NativeWindow.CreateInput());

        SystemAPI.Input = InputManager;
        SystemAPI.WindowSize = new(Size.X, Size.Y);

        InputManager.KeyDown += InputManager_KeyDown;
        InputManager.KeyUp += InputManager_KeyUp;
        InputManager.MouseMove += InputManager_MouseMove;
        InputManager.Click += InputManager_Click;

        Activated?.Invoke(this, EventArgs.Empty);
    }

    private void InputManager_Click(Vector2 position, MouseButton btn)
    {
        OnMouseClick(btn, position);
    }

    private void InputManager_MouseMove(Vector2 position)
    {
        OnMouseMove(position);
    }

    private void InputManager_KeyUp(Key key)
    {
        OnKeyUp(key);
    }

    private void InputManager_KeyDown(Key key)
    {
        OnKeyDown(key);
    }

    private void OnWindowRender(double deltaTime)
    {
        Render(deltaTime);
    }

    private void OnWindowResize(Silk.NET.Maths.Vector2D<int> size)
    {
        Size = size;
        OnResize(size);
    }

    public void Activate()
    {
        NativeWindow.Run();
    }

    public virtual void Dispose()
    {
        InputManager?.Dispose();
        CraftyNative.Dispose();
        GC.SuppressFinalize(this);
    }
}
