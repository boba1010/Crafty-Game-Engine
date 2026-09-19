namespace CraftyNative.TwoD.Animations;

public static class AnimationsManager
{
    internal static List<Animation> Animations { get; } = [];

    public static void Add(Animation animation)
    {
        Animations.Add(animation);
    }
    public static void Remove(Animation animation)
    {
        Animations.Remove(animation);
    }
    public static void Clear()
    {
        Animations.Clear();
    }
}
