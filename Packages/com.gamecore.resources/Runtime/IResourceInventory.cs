namespace GameCore.Resources
{
    public interface IResourceInventory
    {
        ResourceBundle Current { get; }
        bool TryAdd(ResourceBundle bundle);
        bool TryRemove(ResourceBundle bundle);
        bool CanAfford(ResourceBundle cost);
    }
}
