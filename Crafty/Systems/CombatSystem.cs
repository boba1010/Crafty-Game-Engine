
using Crafty.Components;
using CraftyNative.ECS;
using CraftyNative.Scenes;

namespace Crafty.Systems;

public sealed class CombatSystem : IGameplaySystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        var requests = scene.GetEntitiesWith<CombatRequest>().ToArray();

        foreach (var attacker in requests)
        {
            ref var request = ref scene.GetComponent<CombatRequest>(attacker);

            var target = new WorldObject(request.TargetId);
            int damage = request.Damage;

            if (scene.HasComponent<Damageable>(target))
            {
                ref var damageable = ref scene.GetComponent<Damageable>(target);
                damageable.Health = Math.Max(0, damageable.Health - damage);
            }

            scene.RemoveComponent<CombatRequest>(attacker);
        }
    }
}
