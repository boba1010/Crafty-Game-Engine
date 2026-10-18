using CraftyNative.ECS;

namespace Crafty.Components;

public struct Gravity(float acceleration = 40f) : IComponent
{
    public float Acceleration = acceleration;
}
