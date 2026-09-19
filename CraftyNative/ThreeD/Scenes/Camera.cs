using Silk.NET.Windowing;
using System.Numerics;

namespace CraftyNative.ThreeD.Scenes;

public sealed class Camera : IDisposable
{
    private readonly IWindow _window;

    public Camera(IWindow window)
    {
        CraftyNative3D.Initialize(window);
        _window = window;
    }

    public Transform Transform { get; set; }
    public Vector3 Target { get; set; } = Vector3.Zero;

    public float FieldOfView { get; set; } = MathF.PI / 4;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;

    public Matrix4x4 View => Matrix4x4.CreateLookAt(Transform.Position, Target, Vector3.UnitY);
    public Matrix4x4 Projection
    {
        get
        {
            var size = _window.Size;
            return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, size.X / (float)size.Y, NearPlane, FarPlane);
        }
    }

    public void Dispose()
    {
        CraftyNative3D.Dispose();
    }
}