using CraftyNative.ThreeD.Scenes;

namespace CraftyNative.ThreeD.ECS;

public interface ISystem
{
    public void Update(ref Scene scene, double deltaTime);
}
