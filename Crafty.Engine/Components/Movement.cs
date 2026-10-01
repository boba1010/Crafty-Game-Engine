using CraftyNative.ECS;
using System.Numerics;

namespace Crafty.Engine.Components;

public struct Movement : IComponent
{
    public Vector3 Velocity;
    public bool IsFlying;
    public bool IsGrounded;
}
