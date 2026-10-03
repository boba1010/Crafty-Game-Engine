using CraftyNative.ECS;

namespace Crafty.Engine.Components;

public struct Player : IComponent
{
    public GameMode GameMode;
}

public enum GameMode
{
    Creative
}