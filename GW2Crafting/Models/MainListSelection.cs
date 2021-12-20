namespace GW2Crafting.Models
{
    public enum MainListType
    {
        Inventory,
        Recipes,
        Bank,
        Material
    }
    public class MainListSelection
    {
        public MainListType ListType { get; set; }
    }
}
