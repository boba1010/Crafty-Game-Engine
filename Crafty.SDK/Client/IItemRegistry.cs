namespace Crafty.SDK.Client;

public interface IItemRegistry
{
    void Register(Item block);
    Item Get(ushort id);
}
