namespace Crafty.SDK.Client;

public interface IItemRegistry
{
    public void Register(ItemCategory category, Item item);
    Item Get(uint id);
    uint[] GetAllIds();
}
