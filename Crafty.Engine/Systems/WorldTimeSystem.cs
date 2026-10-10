using CraftyNative.ECS;
using CraftyNative.Environment;
using CraftyNative.Scenes;
using System.Numerics;

namespace Crafty.Engine.Systems;

public sealed class WorldTimeSystem : IGameplaySystem
{
    public float DayLengthSeconds { get; set; } = 1800f;
    public float TimeOfDay { get; private set; } = 0.25f;

    public void Update(ref Scene scene, double deltaTime)
    {
        if (DayLengthSeconds <= 0f)
            return;

        TimeOfDay = (float)(TimeOfDay + deltaTime / DayLengthSeconds) % 1f;

        UpdateSun();
        UpdateSky();
    }

    private void UpdateSun()
    {
        float angle = TimeOfDay * MathF.Tau;

        Vector3 direction = Vector3.Normalize(new(0f, MathF.Sin(angle), MathF.Cos(angle)));

        SunRenderer.Direction = direction;
    }

    private void UpdateSky()
    {
        float daylight = Math.Clamp(MathF.Sin(TimeOfDay * MathF.Tau), 0f, 1f);

        SkyRenderer.TopColor = Vector4.Lerp(new(0.015f, 0.02f, 0.07f, 1f), new(0.18f, 0.42f, 0.85f, 1f), daylight);

        SkyRenderer.HorizonColor = Vector4.Lerp(new(0.025f, 0.025f, 0.06f, 1f), new(0.55f, 0.75f, 0.95f, 1f), daylight);
    }
}