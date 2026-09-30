namespace CraftyNative.ECS;

public interface IJobSystem : IDisposable
{
    void Submit(IJob job, int priority = 0, JobFence? fence = null);
    JobFence CreateFence();
}
