using CraftyNative.ECS;

namespace Crafty.Components;

public struct Attacker : IComponent
{
    public int Damage;
    public int Range;
    public double AttackSpeed;
}
