using Gw2Sharp.WebApi.V2.Models;

namespace GW2Crafting.Caching.Models
{
    public class Gw2Currency
    {
        public Gw2Currency() { }
        public Gw2Currency(Currency original)
        {
            Id = original.Id;
            Name = original.Name;
            Icon = original.Icon;
            Order = original.Order;
            Description = original.Description;
        }
        public Gw2Currency(Gw2Currency original)
        {
            Id = original.Id;
            Name = original.Name;
            Icon = original.Icon;
            Order = original.Order;
            Description = original.Description;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
    }
}
