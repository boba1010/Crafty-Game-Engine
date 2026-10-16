using CraftyNative.ECS;

namespace Crafty.Engine.Components;

public struct FallDamage : IComponent
{
    public bool IsTracking;
    public float HighestY;
}
