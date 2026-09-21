using CraftyNative.ThreeD.ECS;

namespace CraftyNative.ThreeD.Scenes;

internal struct HierarchySystem() : ISystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var child in scene.GetEntitiesWith<Transform>())
        {
            var parent = scene.GetParent(child);

            if (parent is null)
                continue;

            ref var parentTransform = ref scene.GetComponent<Transform>(parent.Value);
            ref var childTransform = ref scene.GetComponent<Transform>(child);

            childTransform.Position = parentTransform.Position + childTransform.LocalPosition;

            childTransform.Rotation = parentTransform.Rotation + childTransform.LocalRotation;
        }
    }
}
