using Crafty.Components;
using Crafty.UI;
using CraftyNative.ECS;
using CraftyNative.Scenes;

namespace Crafty.Systems;

public sealed class PlayerSystem : ISystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        var playerObject = scene.GetEntitiesWith<Player>().FirstOrDefault();

        ref var player = ref scene.GetComponent<Player>(playerObject);

        ref var health = ref scene.GetComponent<Damageable>(playerObject);

        if (player.GameMode is GameMode.Survival)
        {
            HudHearts.IsVisible = true;
            HudHearts.SetHealth(health.Health, health.MaxHealth);
        }
        else
        {
            HudHearts.IsVisible = false;
        }
    }
}
