using CraftyNative.ECS;

namespace Crafty.Components;

public struct Player() : IComponent
{
    public GameMode GameMode = GameMode.Survival;
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