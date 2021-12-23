using Gw2Sharp.WebApi.V2.Models;
using LiteDB;

namespace GW2Crafting.Caching.Models
{
    public class Gw2Item
    {
        public Gw2Item() { }
        internal Gw2Item(Item original)
        {
            Id = original.Id;
            Name = original.Name;
            Description = original.Description ?? String.Empty;
            Type = original.Type;
            Level = original.Level;
            Rarity = original.Rarity;
            VendorValue = original.VendorValue;
            Flags = original.Flags.Select(w => w.Value).ToList();
            Icon = original.Icon.Url;
        }        
        internal Gw2Item(Gw2Item original)
        {
            Id = original.Id;
            Name = original.Name;
            Description = original.Description ?? String.Empty;
            Type = original.Type;
            Level = original.Level;
            Rarity = original.Rarity;
            VendorValue = original.VendorValue;
            Flags = original.Flags?.Select(w => w).ToList() ?? Array.Empty<ItemFlag>().ToList();
            Icon = original.Icon;
        }
        public int Id { get; set; }
        public string? Name { get; set; }   
        public string? Description { get; set; }
        public ItemType Type { get; set; }
        public int Level { get; set; }
        public ItemRarity Rarity { get; set; }
        public int VendorValue { get; set; }
        public IEnumerable<ItemFlag>? Flags { get; set; }
        public Uri? Icon { get; set; }

    }
}
