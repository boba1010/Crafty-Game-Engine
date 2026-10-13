using CraftyNative.ECS;

namespace Crafty.Engine.Components;

public struct Player() : IComponent
{
    public GameMode GameMode = GameMode.Creative;
    public Prespective Prespective;
}

public enum Prespective
{
    FirstPerson,
    ThirdPerson
}

public enum GameMode
{
    Creative,
    Survival
}