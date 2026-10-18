using Crafty.Components;
using Crafty.UI;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;

namespace Crafty.Systems;

public sealed class RespawnSystem : IGameplaySystem
{
    private bool _respawnRequested;

    public RespawnSystem()
    {
        HudDeathScreen.RespawnClicked += HudDeathScreen_RespawnClicked;
    }

    private void HudDeathScreen_RespawnClicked()
    {
        _respawnRequested = true;
        HudDeathScreen.Close();
    }

    public void Update(ref Scene scene, double deltaTime)
    {
        if (!_respawnRequested)
            return;

        _respawnRequested = false;

        var playerObject = scene.GetEntitiesWith<Player>().FirstOrDefault();
        if (!scene.HasComponent<Dead>(playerObject) ||
            !scene.HasComponent<Damageable>(playerObject) ||
            !scene.HasComponent<Transform>(playerObject))
            return;

        ref var health = ref scene.GetComponent<Damageable>(playerObject);

        health.Health = health.MaxHealth;

        scene.RemoveComponent<Dead>(playerObject);

        ref var transform = ref scene.GetComponent<Transform>(playerObject);

        transform.Position = new(0, 70, 0);

        GameStateManager.Set(GameState.Playing);
    }
}
