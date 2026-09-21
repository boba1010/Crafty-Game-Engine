using CraftyNative.ThreeD.ECS;

namespace CraftyNative.ThreeD.Scenes;

public struct Camera : IComponent
{
    public Camera()
    {
    }

    public float FieldOfView { get; set; } = MathF.PI / 4;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;
}
