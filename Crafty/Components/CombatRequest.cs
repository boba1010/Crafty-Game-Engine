using CraftyNative.ECS;

namespace Crafty.Components;

public struct CombatRequest : IComponent
{
    public int Damage;
    public uint TargetId;
}