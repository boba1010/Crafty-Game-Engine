using CraftyNative.ECS;

namespace Crafty.Components;

public struct FallDamage : IComponent
{
    public bool IsTracking;
    public float HighestY;
}
