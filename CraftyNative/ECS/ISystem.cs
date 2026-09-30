using CraftyNative.Scenes;

namespace CraftyNative.ECS;

public interface ISystem
{
    public void Update(ref Scene scene, double deltaTime);
}
