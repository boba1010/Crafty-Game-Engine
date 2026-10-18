
using Crafty.Components;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using System.Numerics;

namespace Crafty.Systems;

public sealed class HostileMobAttackSystem : IGameplaySystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        var players = scene.GetEntitiesWith<Player>().ToArray();

        if (players.Length == 0)
            return;

        var player = players[0];

        if (!scene.HasComponent<Transform>(player) || !scene.HasComponent<Damageable>(player))
            return;

        ref var playerTransform = ref scene.GetComponent<Transform>(player);

        foreach (var mob in scene.GetEntitiesWith<HostileMob>().ToArray())
        {
            if (!scene.HasComponent<Attacker>(mob) ||
                !scene.HasComponent<Transform>(mob) ||
                !scene.HasComponent<AttackTimer>(mob))
                continue;

            // Don't overwrite a request that hasn't been processed.
            if (scene.HasComponent<CombatRequest>(mob))
                continue;

            ref var attacker = ref scene.GetComponent<Attacker>(mob);
            ref var timer = ref scene.GetComponent<AttackTimer>(mob);
            ref var mobTransform = ref scene.GetComponent<Transform>(mob);

            if (attacker.AttackSpeed <= 0 || attacker.Damage <= 0)
                continue;

            timer.TimeRemaining -= Math.Max(0, deltaTime);

            float distanceSquared = Vector3.DistanceSquared(mobTransform.Position, playerTransform.Position);

            if (distanceSquared > (float)attacker.Range * attacker.Range)
                continue;

            if (timer.TimeRemaining > 0)
                continue;

            scene.AddComponent(mob, new CombatRequest
            {
                Damage = attacker.Damage,
                TargetId = player.Id
            });

            timer.TimeRemaining = attacker.AttackSpeed;
        }
    }
}
