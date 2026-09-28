namespace CraftyNative.ThreeD.ECS;

public interface IJobSystem : IDisposable
{
    void Submit(IJob job);
}
