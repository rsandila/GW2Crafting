using GW2Crafting.Caching;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;

namespace GW2Crafting.Common
{
    public class Listings
    {
        // Hard coded prices for crafting materials
        private static readonly IReadOnlyDictionary<int, int> _listings = new Dictionary<int, int>
        {
            { 23038,     32 }, // Crude Salvage Kit
            { 23040,     88 }, // "Basic Salvage Kit",
            { 23041,    288 }, // Fine Salvage Kit
            { 23042,    800 }, // "Journeyman's Salvage Kit"
            { 23043,   1536 }, // "Master's Salvage Kit",
            { 22992,     56 }, // "Iron Harvesting Sickle",
            { 22997,    400 }, // "Orichalcum Harvesting Sickle",
            { 23004,     88 }, // "Steel Harvesting Sickle",
            { 23005,    120 }, // "Darksteel Harvesting Sickle",
            { 23008,    160 }, // "Mithril Harvesting Sickle"
            { 23013,    160 }, // "Mithril Harvesting Sickle"
            { 23029,     24 }, // "Copper Harvesting Sickle",
            { 64114,     24 }, // "Copper Harvesting Sickle",
            { 22994,     56 }, // "Iron Logging Axe"
            { 23000,    400 }, // "Orichalcum Logging Axe"
            { 23002,     88 }, // "Steel Logging Axe"
            { 23006,    120 }, // "Darksteel Logging Axe"
            { 23009,    160 }, // "Mithril Logging Axe"
            { 23030,     24 }, // "Copper Logging Axe"
            { 64101,     24 }, // "Copper Logging Axe"
            { 22995,     56 }, // "Iron Mining Pick"
            { 23001,    400 }, // "Orichalcum Mining Pick"
            { 23003,     88 }, // "Steel Mining Pick"
            { 23007,    120 }, // "Darksteel Mining Pick"
            { 23010,    160 }, // "Mithril Mining Pick"
            { 23011,    160 }, // "Mithril Mining Pick"
            { 23031,     24 }, // "Copper Mining Pick"
            { 64115,     24 }, // "Copper Mining Pick",
            { 62942,      8 }, // "Crafter's Backpack Frame"
            { 19789,    160 }, // "Spool of Wool Thread"
            { 19790,    640 }, // "Spool of Gossamer Thread"
            { 19791,    480 }, // "Spool of Silk Thread"
            { 19792,     80 }, // "Spool of Jute Thread"
            { 19793,    320 }, // "Spool of Linen Thread"
            { 19794,    240 }, // "Spool of Cotton Thread"
            { 13006,   1480 }, // "Rune of Holding"
            { 13007,     50 }, // "Major Rune of Holding"
            { 13008,  20000 }, // "Greater Rune of Holding"
            { 13009, 100000 }, // "Superior Rune of Holding"
            { 13010,   1480 }, // "Minor Rune of Holding"
            { 46747,   1496 }, // "Thermocatalytic Reagent" 
            { 19704,     80 }, // "Lump of Tin"
            { 19750,    160 }, // "Lump of Coal"
            { 19924,    480 }, // "Lump of Primordium"
            { 12156,     80 }, // "Jug of Water"
            { 12136,     80 }, // "Bag of Flour"
            { 12155,     80 }, // "Bag of Sugar"
            { 12324,     80 }, // "Bag of Starch"
            { 12271,     80 }, // "Bottle of Soy Sauce"
            { 12157,     80 }, // "Jar of Vinegar"
            { 12158,     80 }, // "Jar of Vegetable Oil"
            { 76839,     56 }, // "Milling Basin"
            { 12151,     80 }, // "Packet of Baking Powder"
            { 12153,     80 }, // "Packet of Salt"
            { 91739,   1496 }, // "Pile of Compost Starter"
            { 91702,   1000 }, // "Pile of Powdered Gelatin Mix"
            { 75762,    140 }, // "Bag of Mortar"
            { 70647,     32 }, // "Crystalline Bottle"
            { 75087,   5000 } // "Essence of Elegance"
        };
        public static IEnumerable<CommerceListings> GetListingsFor(Gw2TokenCache tokenCache, IEnumerable<int> listingIds)
        {
            if (listingIds == null || !listingIds.Any())
            {
                return Array.Empty<CommerceListings>();
            }
            var apiListings = tokenCache.GetListingsFor(listingIds).ToDictionary(w => w.Id, w => w);
            foreach (var listingId in _listings.Keys.Where(w => listingIds.Contains(w)))
            {
                if (apiListings.ContainsKey(listingId))
                {
                    if (apiListings[listingId].GetSellingUnitPrice(0, 1) >= _listings[listingId])
                    {
                        continue;
                    }
                }
                apiListings[listingId] = new CommerceListings { Id = listingId, Sells = new List<CommerceListing> {  new CommerceListing { Listings = 1, Quantity = 1000000, UnitPrice = _listings[listingId] } }, Buys = Array.Empty<CommerceListing>() };
            }
            return apiListings.Values.ToList();
        }

        internal async static Task CacheListingsFor(Gw2TokenCache tokenCache, Guid id, IEnumerable<int> listingIds)
        {
            await tokenCache.CacheCommerceListings(listingIds.Where(w => w != 0), id);
        }
        internal static string DisplayablePrice(int price)
        {
            int copper = price % 100;
            int silver = (price / 100) % 100;
            int gold = (price / 10000);
            if (gold != 0)
            {
                return $"{gold}g {silver}s {copper}c";
            }
            if (silver != 0)
            {
                return $"{silver}s {copper}c";
            }
            return $"{copper}c";
        }
    }
}
