using CraftyNative.ECS;

namespace Crafty.Components;

public struct Damageable() : IComponent
{
    public int Health = 20;
    public int MaxHealth = 20;
}
