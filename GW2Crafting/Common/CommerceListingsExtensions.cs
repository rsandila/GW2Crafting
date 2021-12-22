using Gw2Sharp.WebApi.V2.Models;

namespace GW2Crafting.Common
{
    public static class CommerceListingsExtensions
    {
        public static int GetSellingUnitPrice(this IEnumerable<CommerceListings> listings, int defaultValue = 0)
        {
            if (listings == null)
            {
                return 0;
            }
            return listings.FirstOrDefault()?.GetSellingUnitPrice(defaultValue, 1) ?? defaultValue;
        }
        public static int GetSellingUnitPrice(this CommerceListings listings, int defaultValue, int numberOfItems)
        {
            if (!(listings?.Buys?.Any() ?? false))
            {
                return defaultValue;
            }
            int total = 0;
            int required = numberOfItems;
            foreach (var item in listings.Buys.OrderByDescending(w => w.UnitPrice))
            {
                if (item.Quantity >= required)
                {
                    total += item.UnitPrice * required;
                    break;
                }
                else
                {
                    total += item.UnitPrice * item.Quantity;
                    required -= item.Quantity;
                }
            }
            return total;
        }
    }
}
