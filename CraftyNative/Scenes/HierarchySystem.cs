using CraftyNative.ECS;
using CraftyNative.ThreeD;
using System.Numerics;

namespace CraftyNative.Scenes;

// Class (not struct) so the scratch list below survives between frames.
// Scene's constructor does Systems.Add(new HierarchySystem()), which still compiles.
internal sealed class HierarchySystem : ISystem
{
    private readonly List<(WorldObject Obj, int Depth)> _ordered = [];

    public void Update(ref Scene scene, double deltaTime)
    {
        // Collect every child with its depth so parents are always processed before
        // their children (player -> camera -> hand), regardless of component-store order.
        _ordered.Clear();
        foreach (var entity in scene.GetEntitiesWith<Transform>())
        {
            int depth = 0;
            var p = scene.GetParent(entity);
            while (p is not null)
            {
                depth++;
                p = scene.GetParent(p.Value);
            }

            if (depth > 0)
                _ordered.Add((entity, depth));
        }

        _ordered.Sort((a, b) => a.Depth.CompareTo(b.Depth));

        foreach (var (child, _) in _ordered)
        {
            var parent = scene.GetParent(child)!.Value;
            ref var parentTransform = ref scene.GetComponent<Transform>(parent);
            ref var childTransform = ref scene.GetComponent<Transform>(child);

            // Rotation the child actually inherits (respects the lock flags)
            var parentRot = parentTransform.Rotation;
            var inherited = Vector3.Zero;
            if (!childTransform.LockXAxis) inherited.X = parentRot.X;
            if (!childTransform.LockYAxis) inherited.Y = parentRot.Y;
            if (!childTransform.LockZAxis) inherited.Z = parentRot.Z;

            var offset = childTransform.LocalPosition;

            if (childTransform.RotateAroundParent)
            {
                // Euler order must match what your renderer uses (Y = yaw, X = pitch, Z = roll)
                var q = Quaternion.CreateFromYawPitchRoll(inherited.Y, inherited.X, inherited.Z);
                offset = Vector3.Transform(offset, q);
            }

            childTransform.Position = parentTransform.Position + offset;
            childTransform.Rotation = childTransform.LocalRotation + inherited;
        }
    }
}