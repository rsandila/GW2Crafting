using GW2Crafting.Caching;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;

namespace GW2Crafting.Common
{
    public class Listings
    {
        public static IEnumerable<CommerceListings> GetListingsFor(Gw2TokenCache tokenCache, Guid id, IEnumerable<int> listingIds)
        {
            if (listingIds == null || !listingIds.Any())
            {
                return Array.Empty<CommerceListings>();
            }
            return tokenCache.GetListingsFor(listingIds);
        }

        internal async static Task CacheListingsFor(Gw2TokenCache tokenCache, Guid id, IEnumerable<int> listingIds)
        {
            await tokenCache.CacheCommerceListings(listingIds, id);
        }
        internal static string DisplayablePrice(int price)
        {
            int copper = price % 100;
            int silver = (price / 100) % 100;
            int gold = (price / 10000);
            if (gold > 0)
            {
                return $"{gold}g {silver}s {copper}c";
            }
            if (silver > 0)
            {
                return $"{silver}s {copper}c";
            }
            return $"{copper}c";
        }
    }
}
