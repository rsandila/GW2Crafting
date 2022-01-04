using GW2Crafting.Caching;
using GW2Crafting.Caching.Models;
using GW2Crafting.Common;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class MaterialItem: Gw2Item
    {
        internal MaterialItem(Item original, string categoryName, long count, long unitSellPrice, long unitBuyPrice) : base(original)
        {
            CategoryName = categoryName;
            Count = count;
            UnitSellPrice = unitSellPrice;
            UnitBuyPrice = unitBuyPrice;
        }
        internal MaterialItem(MaterialItem original)
        {
            CategoryName = original.CategoryName;
            Count = original.Count;
            UnitSellPrice = original.UnitSellPrice;
            Id = original.Id;
            Name = original.Name;
            Description = original.Description ?? String.Empty;
            Type = original.Type;
            Level = original.Level;
            Rarity = original.Rarity;
            VendorValue = original.VendorValue;
            Flags = original.Flags?.Select(w => w).ToList() ?? Array.Empty<ItemFlag>().ToList();
            Icon = original.Icon;
            UnitBuyPrice = original.UnitBuyPrice;
        }

        public MaterialItem(Gw2Item original, string categoryName, int count, int unitSellPrice, int unitBuyPrice) : base(original)
        {
            CategoryName = categoryName;
            Count = count;
            UnitSellPrice = unitSellPrice;
            UnitBuyPrice= unitBuyPrice;
        }

        public string CategoryName { get; set; }
        public long Count { get; set; }
        public long UnitSellPrice { get; set; }
        public long UnitBuyPrice { get; set; }
    }
    public class MaterialModel : PageModel
    {
        private readonly ILogger<MaterialModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        [BindProperty]
        public List<MaterialItem> Items { get; }

        public MaterialModel(ILogger<MaterialModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
            Items = new List<MaterialItem>();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id == Guid.Empty)
            {
                return RedirectToPage("Index");
            }
            var client = await _tokenCache.Get<Gw2Client>(id, CacheTypeId.Client);
            if (client == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var materials = await _tokenCache.Get<IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.MaterialCategory>>(id, CacheTypeId.MaterialCategories);
            if (materials == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var categoryDict = materials.ToDictionary(x => x.Id, x => x);
            var material = await _tokenCache.Get<IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.AccountMaterial>>(id, CacheTypeId.Materials);
            if (material == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var listings = Listings.GetListingsFor(_tokenCache, material.Select(x => x.Id));
            if (!listings.Any())
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var listingDict = listings.ToDictionary(x => x.Id, x => x.Sells.OrderBy(w => w.UnitPrice).FirstOrDefault());
            foreach (var item in material)
            {
                var resolvedItem = _database.GetItem(item.Id);
                if (resolvedItem == null)
                {
                    continue;
                }
                string categoryName = string.Empty;
                if (categoryDict.TryGetValue(item.Category, out var category))
                {
                    categoryName = category.Name;
                }
                Items.Add(new MaterialItem(resolvedItem, categoryName, item.Count, listings.GetSellingUnitPrice(0), listings.GetBuyingUnitPrice(0)));
            }
            return Page();
        }
    }
}
