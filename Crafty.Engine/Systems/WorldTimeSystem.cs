using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Environment;
using CraftyNative.Scenes;
using System.Numerics;

namespace Crafty.Engine.Systems;

public sealed class WorldTimeSystem : IGameplaySystem
{
    //public float DayLengthSeconds { get; set; } = 1800f;
    public float DayLengthSeconds { get; set; } = 60f;
    public float TimeOfDay { get; private set; } = 0.25f;
    public static float Daylight => Math.Clamp(SunRenderer.Direction.Y, 0f, 1f);
    public float Moonlight => Math.Clamp(-SunRenderer.Direction.Y, 0f, 1f);

    public void Update(ref Scene scene, double deltaTime)
    {
        if (DayLengthSeconds <= 0f)
            return;

        TimeOfDay = (float)(TimeOfDay + deltaTime / DayLengthSeconds) % 1f;

        UpdateSun();
        UpdateSky();

        CraftyNative.CraftyNative.Daylight = Daylight;
        CraftyNative.CraftyNative.Moonlight = Moonlight;
    }

    private void UpdateSun()
    {
        float angle = TimeOfDay * MathF.Tau;

        Vector3 direction = Vector3.Normalize(new(0f, MathF.Sin(angle), MathF.Cos(angle)));

        SunRenderer.Direction = direction;
    }


    private void UpdateSky()
    {
        // 0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset.
        float daylight = Math.Clamp(MathF.Sin(TimeOfDay * MathF.Tau), 0f, 1f);

        // Strong orange glow near sunrise and sunset.
        float twilight = MathF.Pow(1f - MathF.Abs(daylight * 2f - 1f), 8f);

        Vector4 nightTop = new(0.015f, 0.025f, 0.09f, 1f);
        Vector4 dayTop = new(0.18f, 0.42f, 0.85f, 1f);
        Vector4 twilightTop = new(0.35f, 0.25f, 0.48f, 1f);

        Vector4 nightHorizon = new(0.025f, 0.035f, 0.10f, 1f);
        Vector4 dayHorizon = new(0.55f, 0.75f, 0.95f, 1f);
        Vector4 twilightHorizon = new(1f, 0.32f, 0.10f, 1f);

        SkyRenderer.TopColor = Vector4.Lerp(Vector4.Lerp(nightTop, dayTop, daylight), twilightTop, twilight);

        SkyRenderer.HorizonColor = Vector4.Lerp(Vector4.Lerp(nightHorizon, dayHorizon, daylight), twilightHorizon, twilight);
    }
}