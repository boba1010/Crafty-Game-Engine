namespace CraftyNative.TwoD.Animations;

public sealed class Animation
{
    public float Duration { get; }
    public float Elapsed { get; private set; }

    public Action<float> Update { get; }

    public bool IsCompleted => Elapsed >= Duration;

    public Animation(float duration, Action<float> update)
    {
        Duration = duration;
        Update = update;
    }

    public void Tick(double deltaTime)
    {
        Elapsed += (float)deltaTime;

        var progress = Math.Clamp(Elapsed / Duration, 0, 1);
        Update(progress);
    }
}
