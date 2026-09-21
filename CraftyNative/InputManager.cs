using Silk.NET.Input;
using System.Numerics;

namespace CraftyNative;

public sealed class InputManager : IDisposable
{
    public static Vector2 MousePosition { get; internal set; }
    public static Vector2 MouseDelta { get; internal set; }
    public Cursor CursorMode
    {
        get => (Cursor)_mouse.Cursor.CursorMode;
        set => _mouse.Cursor.CursorMode = (CursorMode)value;
    }
    public event Action<Key>? KeyDown;
    public event Action<Key>? KeyUp;
    public event Action<Vector2>? MouseMove;
    private readonly IInputContext _input;
    private readonly IKeyboard _keyboard;
    private readonly IMouse _mouse;
    private bool _hasMousePosition;

    public InputManager(IInputContext input)
    {
        _input = input;

        _keyboard = _input.Keyboards[0];
        _mouse = _input.Mice[0];

        _keyboard.KeyDown += Keyboard_KeyDown;
        _keyboard.KeyUp += Keyboard_KeyUp;
        _mouse.MouseMove += Mouse_MouseMove;
    }

    public void CenterMouse(Vector2 windowSize)
    {
        var center = windowSize / 2f;

        _mouse.Position = center;
        MousePosition = center;
        MouseDelta = Vector2.Zero;
    }

    private void Mouse_MouseMove(IMouse mouse, Vector2 position)
    {
        if (!_hasMousePosition)
        {
            MousePosition = position;
            MouseDelta = Vector2.Zero;
            _hasMousePosition = true;
        }
        else
        {
            MouseDelta = position - MousePosition;
            MousePosition = position;
        }
        MouseMove?.Invoke(position);
    }

    private void Keyboard_KeyUp(IKeyboard arg1, Silk.NET.Input.Key arg2, int arg3)
    {
        KeyUp?.Invoke((Key)arg2);
    }

    private void Keyboard_KeyDown(IKeyboard arg1, Silk.NET.Input.Key arg2, int arg3)
    {
        KeyDown?.Invoke((Key)arg2);
    }

    public bool IsKeyDown(Key key) 
    {
        return _keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
    }

    public bool IsMouseButtonDown(MouseButton button) 
    {
        return _mouse.IsButtonPressed((Silk.NET.Input.MouseButton)button);
    }

    public void Dispose()
    {
        var keyboard = _input.Keyboards[0];
        var mouse = _input.Mice[0];

        _keyboard.KeyDown -= Keyboard_KeyDown;
        _keyboard.KeyUp -= Keyboard_KeyUp;
        _mouse.MouseMove -= Mouse_MouseMove;
    }
}
