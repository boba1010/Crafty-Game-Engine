using Crafty.Engine.Components;
using Crafty.Engine.UI;
using CraftyNative.ECS;
using CraftyNative.Scenes;

namespace Crafty.Engine.Systems;

public sealed class PlayerSystem : ISystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        var playerObject = scene.GetEntitiesWith<Player>().FirstOrDefault();

        ref var player = ref scene.GetComponent<Player>(playerObject);

        if (player.GameMode is GameMode.Survival)
        {
            HudHearts.IsVisible = true;
        }
        else
        {
            HudHearts.IsVisible = false;
        }
    }
}
